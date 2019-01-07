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

        /// <summary>
        /// Whether the app runs in dev/test/staging/prod.
        /// </summary>
        internal string Environment { get; set; }

        protected LogEntrySink(int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
        {
        }
        
        /// <summary>
        /// Extracts logged entries and forwards them for serialization.
        /// </summary>
        protected override Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            IEnumerable<LogEntryDto> logEntries = events.Select(ExtractLogEntry);
            return WriteLogEntries(logEntries);
        }

        /// <summary>
        /// Performs the actual serialization / logging of a batch of log entries.
        /// </summary>
        protected abstract Task WriteLogEntries(IEnumerable<LogEntryDto> entryDtos);

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
                    //no log entry to unwrap - assume just a regular Serilog message that didn't come through the custom API
                    var logLevel = ParseLevel(logEvent.Level);
                    logEntry = new LogEntry
                    {
                        TimestampOverride = logEvent.Timestamp,
                        LogLevel = logLevel,
                        Message = logEvent.RenderMessage(),
                        Exception = logEvent.Exception,
                        Context = $"{AppName}.{logLevel}"
                    };
                }

                //extract all other properties and add them to the context object
                var props = logEvent.Properties.Where(p => !p.Key.Equals(Logger.EntryPropertyName));
                foreach (var prop in props)
                {
                    var scalarValue = prop.Value as ScalarValue;
                    if (scalarValue != null)
                    {
                        //insert (override duplicate keys)
                        logEntry.ContextData[prop.Key] = scalarValue.Value;
                    }
                }

                var dto = LogEntryParser.ParseLogEntry(logEntry, AppName, Environment);
                return dto;
            }
            catch (Exception e)
            {
                return ProcessLoggingException(e);
            }
        }

        private LogLevel ParseLevel(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Debug:
                case LogEventLevel.Verbose:
                    return LogLevel.Debug;
                case LogEventLevel.Information:
                    return LogLevel.Info;
                case LogEventLevel.Warning:
                    return LogLevel.Warning;
                case LogEventLevel.Error:
                    return LogLevel.Error;
                default:
                    return LogLevel.Fatal;
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
                AppName = AppName,
                Environment = Environment,
                Level = LogLevel.Fatal.ToString(),
                Context = "Logging.Error",
                PayloadType = "Logging.Error",
                Payload = new { Error = e.ToString() }
            };
        }
    }
}
