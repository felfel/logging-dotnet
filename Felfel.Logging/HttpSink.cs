using System;
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
        /// <param name="endpointUri">Endpoint that contains the access token.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="clientBuilder">Optional client builder, which can be used to customize client
        /// calls (e.g. with custom request headers).</param>
        public HttpSink(string endpointUri, int batchSizeLimit, TimeSpan period, Func<HttpClient> clientBuilder = null) : base(batchSizeLimit, period)
        {
            EndpointUri = endpointUri;
            Client = clientBuilder == null ? new HttpClient() : clientBuilder();

            SnakeCasing = new SnakeCaseNamingStrategy();
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = SnakeCasing
            };

            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None
            };
        }

        /// <summary>
        /// Triggers individual HTTP Posts to the configured endpoint
        /// for each batched log entry.
        /// </summary>
        protected override Task WriteLogEntry(LogEntryDto entryDto)
        {
            var content = GetHttpContent(entryDto);
            return Client.PostAsync(EndpointUri, content);
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

        private HttpContent CreateHttpContent(LogEntryDto content)
        {
            var json = JsonConvert.SerializeObject(content, JsonSerializerSettings);

            //customize the "data" property and use the document type as a property key instead.
            //this prevents indexing errors if two completely payloads have matching keys,
            //or if the data actually is a scalar value already.
            if (content.Data != null)
            {
                string propertyName = String.IsNullOrEmpty(content.PayloadType) ? content.Context : content.PayloadType;
                propertyName = propertyName.Replace(".", "_");
                propertyName = SnakeCasing.GetPropertyName(propertyName, false);
                json = json.Replace(LogEntryDto.DataPropertyPlaceholderName, propertyName);
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