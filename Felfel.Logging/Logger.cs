using System;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;

namespace Felfel.Logging
{
    /// <summary>
    /// Primary façade for structured logging.
    /// <remarks>Having static builder methods is not a good practice, but will do for now given that
    /// we do not leverate dependency injection on our legacy infrastructure. You probably should not
    /// duplicate this pattern.</remarks>
    /// </summary>
    public class Logger
    {
        internal const string EntryPropertyName = nameof(LogEntry);

        private static readonly MessageTemplate EmptyMessageTemplate =
            new MessageTemplate("", Enumerable.Empty<MessageTemplateToken>());

        /// <summary>
        /// Logger context, which will be part of the serialized data unless explicitly
        /// set through the <see cref="LogEntry.Context"/> property.
        /// </summary>
        public string Context { get; }

        private Logger(string context)
        {
            Context = context;
        }

        /// <summary>
        /// Creates a new logger instance, using the
        /// type parameter as the logger's <see cref="Context"/>.
        /// </summary>
        public static Logger Create<T>()
        {
            return Create(typeof(T));
        }

        /// <summary>
        /// Creates a new logger instance, using the
        /// specified <paramref name="contextType"/> 
        /// as the logger's <see cref="Context"/>.
        /// </summary>
        public static Logger Create(Type contextType)
        {
            return Create(contextType.Name);
        }

        /// <summary>
        /// Creates a new logger instance for a given context.
        /// </summary>
        /// <returns></returns>
        public static Logger Create(string context = "")
        {
            return new Logger(context);
        }

        /// <summary>
        /// Schedules a new log entry for logging. You probably should use one of the convenience
        /// overloads such as <c>Warn</c>, <c>Info</c> or <c>Debug</c> that can be found
        /// in <see cref="LoggerExtensions"/>.
        /// </summary>
        public void Log(LogEntry entry)
        {
            if (String.IsNullOrEmpty(entry.Context))
            {
                entry.Context = Context;
            }

            //send to Serilog (will be unwrapped later again by our custom sink)
            var prop = new LogEventProperty(EntryPropertyName, new ScalarValue(entry));
            LogEventProperty[] props = {prop};
            LogEvent le = new LogEvent(DateTimeOffset.Now, Parse(entry.LogLevel), null, EmptyMessageTemplate, props);
            Serilog.Log.Logger.Write(le);

            
            LogEventLevel Parse(LogLevel level)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        return LogEventLevel.Debug;
                    case LogLevel.Info:
                        return LogEventLevel.Information;
                    case LogLevel.Warning:
                        return LogEventLevel.Warning;
                    case LogLevel.Error:
                        return LogEventLevel.Error;
                    default:
                        return LogEventLevel.Fatal;
                }
            }
        }
    }
}