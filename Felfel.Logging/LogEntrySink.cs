using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Felfel.Logging
{
    /// <summary>
    /// Base class that extacts <see cref="LogEntry"/> instances from
    /// Serilog and triggers async batched serialization.
    /// </summary>
    public abstract class LogEntrySink : PeriodicBatchingSink
    {
        /// <summary>
        /// An optional application / service name that can be used as an identifier
        /// for all logging coming out of a given application regardless the context.
        /// </summary>
        internal string AppName { get; set; }

        protected LogEntrySink(int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
        {
        }


        /// <summary>
        /// Extracts logged entries and forwards them for serialization.
        /// </summary>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var tasks = events
                .Select(ExtractLogEntry)
                .Select(WriteLogEntry)
                .Where(t => t != null);

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }


        /// <summary>
        /// Performs the actual serialization / logging of a log entry.
        /// </summary>
        protected abstract Task WriteLogEntry(LogEntryDto entryDto);

        /// <summary>
        /// Parses a Serilog <see cref="LogEvent"/> and extracts the
        /// underlying <see cref="LogEntry"/> which is then transformed
        /// in a serializable DTO.
        /// </summary>
        protected virtual LogEntryDto ExtractLogEntry(LogEvent logEvent)
        {
            try
            {
                logEvent.Properties.TryGetValue(Logger.EntryPropertyName, out var entryProperty);
                var logEntry = (entryProperty as ScalarValue)?.Value as LogEntry;

                if (logEntry == null)
                {
                    //something went wrong
                    logEntry = new LogEntry
                    {
                        TimestampOverride = logEvent.Timestamp,
                        LogLevel = LogLevel.Fatal,
                        Exception = logEvent.Exception,
                        Context = $"{nameof(HttpSink)}.Error",
                        Payload = new
                        {
                            Error = "Could not unwrap log entry.",
                            Message = logEvent.RenderMessage(),
                            Level = logEvent.Level.ToString()
                        }
                    };
                }

                var dto = LogEntryParser.ParseLogEntry(logEntry);
                dto.AppName = AppName;
                return dto;
            }
            catch (Exception e)
            {
                return ProcessLoggingException(e);
            }
        }


        /// <summary>
        /// Wraps an exception that occurred during logging into
        /// a loggable <see cref="LogEntryDto"/>. 
        /// </summary>
        protected virtual LogEntryDto ProcessLoggingException(Exception e)
        {
            return new LogEntryDto
            {
                Timestamp = DateTimeOffset.Now,
                Level = LogLevel.Fatal.ToString(),
                Context = "Logging.Error",
                PayloadType = "Logging.Error",
                Payload = new { Error = e.ToString() }
            };
        }
    }
}