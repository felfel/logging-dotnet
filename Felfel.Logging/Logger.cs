using System;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;

namespace Felfel.Logging
{
    public class Logger<T> : Logger, ILogger<T>
    {
        /// <summary>
        /// Creates a new logger instance using the type name
        /// as the <see cref="ILogger.Context"/>.
        /// </summary>
        /// <param name="prefixPayloadType">If true, the <see cref="LogEntry.PayloadType"/>
        /// will be automatically prefixed with the logger's <see cref="ILogger.Context"/> in order
        /// to create a qualified payload name, which reduces the risk of potential
        /// duplicates across app/service.
        /// </param>
        public Logger(bool prefixPayloadType = true) : base(typeof(T).Name, prefixPayloadType)
        {
        }
    }

    /// <summary>
    /// Primary façade for structured logging.
    /// <remarks>Having static builder methods is not a good practice, but will do for now given that
    /// we do not leverate dependency injection on our legacy infrastructure. You probably should not
    /// duplicate this pattern.</remarks>
    /// </summary>
    public class Logger : ILogger
    {
        internal const string EntryPropertyName = nameof(LogEntry);

        private static readonly MessageTemplate EmptyMessageTemplate =
            new MessageTemplate("", Enumerable.Empty<MessageTemplateToken>());

        /// <summary>
        /// Logger context, which will be part of the serialized data unless explicitly
        /// set through the <see cref="LogEntry.Context"/> property. Identifies messages
        /// that belong together (along with the <see cref="LogEntry.PayloadType"/> at finer
        /// granularity).
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// Concatenates the the <see cref="Context"/> and the
        /// <see cref="LogEntry.PayloadType"/> of a log entry
        /// in order to ensure a qualified (and unique) payload type name.
        /// </summary>
        public bool PrefixPayloadType { get; set; }

        protected Logger(string context, bool prefixPayloadType)
        {
            Context = context;
            PrefixPayloadType = prefixPayloadType;
        }

        /// <summary>
        /// Creates a new logger instance, using the
        /// type parameter as the logger's <see cref="Context"/>.
        /// </summary>
        /// <typeparam name="T">Uses the specified type's name as the logger's
        /// <see cref="Context"/>.</typeparam>
        /// <param name="prefixPayloadType">If true, the <see cref="LogEntry.PayloadType"/>
        /// will be automatically prefixed with the logger's <see cref="Context"/> in order
        /// to create a qualified payload name, which reduces the risk of potential
        /// duplicates across app/service.
        /// </param>
        public static ILogger<T> Create<T>(bool prefixPayloadType = true)
        {
            return new Logger<T>(prefixPayloadType);
        }

        /// <summary>
        /// Creates a new logger instance, using the
        /// specified <paramref name="contextType"/> 
        /// as the logger's <see cref="Context"/>.
        /// </summary>
        /// <param name="contextType">Uses the specified type's name as the
        /// <see cref="Context"/>.</param>
        /// <param name="prefixPayloadType">If true, the <see cref="LogEntry.PayloadType"/>
        /// will be automatically prefixed with the logger's <see cref="Context"/> in order
        /// to create a qualified payload name, which reduces the risk of potential
        /// duplicates across app/service.
        /// </param>
        public static ILogger Create(Type contextType, bool prefixPayloadType = true)
        {
            return Create(contextType.Name, prefixPayloadType);
        }

        /// <summary>
        /// Creates a new logger instance for a given context.
        /// </summary>
        /// <param name="context">A context identifier which can be used to query
        /// messages that belong together. This is the logger's "name" in many
        /// logging frameworks. </param>
        /// <param name="prefixPayloadType">If true, the <see cref="LogEntry.PayloadType"/>
        /// will be automatically prefixed with the logger's <see cref="Context"/> in order
        /// to create a qualified payload name, which reduces the risk of potential
        /// duplicates across app/service.
        /// </param>
        public static ILogger Create(string context = "", bool prefixPayloadType = true)
        {
            return new Logger(context, prefixPayloadType);
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

            if (PrefixPayloadType && !String.IsNullOrEmpty(entry.PayloadType) && !String.IsNullOrEmpty(Context))
            {
                entry.PayloadType = $"{entry.Context}.{entry.PayloadType}";
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