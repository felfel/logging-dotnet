using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Felfel.Logging
{
    /// <summary>
    /// Simple logging sink that write straight to an HTTP(S) endpoint. Doesn't
    /// cater to offline scenarios, dropped messages or anything, but
    /// it'll do for the little things we do in .NET world.
    /// </summary>
    public class HttpSink : LogEntrySink
    {
        public string EndpointUri { get; }

        public HttpClient Client { get; }

        public SnakeCaseNamingStrategy SnakeCasing { get; }

        public JsonSerializerSettings JsonSerializerSettings { get; }

        /// <summary>Creates the sink for a given collector endpoint.</summary>
        /// <param name="appName">Application or service name.</param>
        /// <param name="environment">Runtime environment (e.g. DEV or PROD).</param>
        /// <param name="endpointUri">Endpoint that contains the access token.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="clientBuilder">Optional client builder, which can be used to customize client
        /// calls (e.g. with custom request headers).</param>
        public HttpSink(string appName, string environment, string endpointUri, int batchSizeLimit, TimeSpan period,
            Func<HttpClient> clientBuilder = null) : base(batchSizeLimit, period)
        {
            EndpointUri = endpointUri;
            Client = clientBuilder == null ? new HttpClient() : clientBuilder();
            AppName = appName;
            Environment = environment;

            SnakeCasing = new SnakeCaseNamingStrategy();
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = SnakeCasing
            };

            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None //ensures everything is on one line
            };
        }

        /// <summary>
        /// Serializes all entries in the batch, and sends them in a single HTTP POST.
        /// </summary>
        /// <param name="entryDtos"></param>
        /// <returns></returns>
        protected override Task WriteLogEntries(IEnumerable<LogEntryDto> entryDtos)
        {
            //send the whole batch in bulk - quotas are so big, we don't have to worry about
            //packages that are too big at this point (bulk size can be configured after all)
            IEnumerable<string> serializedEntries = entryDtos
                .Select(dto => GetLogEntryJson(dto))
                .Where(s => !String.IsNullOrEmpty(s));

            var json = String.Join("\n", serializedEntries);

            //create string content - it's lines of JSON, but not a JSON document itself, so content type is text/plain
            var httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return Client.PostAsync(EndpointUri, httpContent);
        }


        private HttpContent GetHttpContent(LogEntryDto dto)
        {
            try
            {
                return CreateHttpContent(dto);
            }
            catch (Exception e)
            {
                dto = ProcessLoggingException(e);
                return CreateHttpContent(dto);
            }
        }

        private string GetLogEntryJson(LogEntryDto logEntry, bool wrapSerializationException = true)
        {
            try
            {
                string json = JsonConvert.SerializeObject(logEntry, JsonSerializerSettings);

                //customize the "data" property and use the document type as a property key instead.
                //this prevents indexing errors if two completely payloads have matching keys,
                //or if the data actually is a scalar value already.
                if (logEntry.Payload != null)
                {
                    string propertyName = String.IsNullOrEmpty(logEntry.PayloadType) ? logEntry.Context : logEntry.PayloadType;
                    propertyName = propertyName.Replace(".", "_");
                    propertyName = SnakeCasing.GetPropertyName(propertyName, false);
                    json = json.Replace(LogEntryDto.PayloadPropertyPlaceholderName, propertyName);
                }

                return json;
            }
            catch (Exception e)
            {
                //if something went wrong, return the exception as a serialized log entry
                if (wrapSerializationException)
                {
                    var dto = ProcessLoggingException(e);
                    return GetLogEntryJson(dto, false);
                }

                //we couldn't even serialize the exception DTO. Can't happen, but make sure we
                //don't end up recursing indefinitely in case we introduce a severe bug
                return null;
            }
        }

        private HttpContent CreateHttpContent(LogEntryDto content)
        {
            string json = JsonConvert.SerializeObject(content, JsonSerializerSettings);

            //customize the "data" property and use the document type as a property key instead.
            //this prevents indexing errors if two completely payloads have matching keys,
            //or if the data actually is a scalar value already.
            if (content.Payload != null)
            {
                string propertyName = String.IsNullOrEmpty(content.PayloadType) ? content.Context : content.PayloadType;
                propertyName = propertyName.Replace(".", "_");
                propertyName = SnakeCasing.GetPropertyName(propertyName, false);
                json = json.Replace(LogEntryDto.PayloadPropertyPlaceholderName, propertyName);
            }

            var httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return httpContent;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Client.Dispose();
            }
        }
    }
}