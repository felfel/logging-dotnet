using System;
using System.Collections.Generic;

namespace Felfel.Logging
{
    public class LogEntry
    {
        /// <summary>
        /// Timestamp, if needed. By default, will just be set
        /// to the current time.
        /// </summary>
        public DateTimeOffset TimestampOverride { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// The application / application area where the logged
        /// message originates from. Used to simplify filtering
        /// and identification of issues in code.
        /// <remarks>
        /// Use dotted notation, e.g. "PaymentApi.CardValidation".
        /// </remarks>
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// This is an identifier of the data that is actually being logged.
        /// While the <see cref="Context"/> may be sufficient in many scenarios,
        /// you may want to log different kinds of information in the same
        /// context. Use simple identifiers ("UserLogin") or dotted notation,
        /// e.g. "Accounts.UserLogin" if the <see cref="Context"/> doesn't provide
        /// that information already.
        /// </summary>
        /// <remarks>
        /// Also, log messages with a given <see cref="PayloadType"/> should always
        /// have an identical schema. For example, do not post a debug message
        /// about a login with the same context as an error message about a failed
        /// login, if those messages have different schemas.</remarks>
        public string PayloadType { get; set; }

        /// <summary>
        /// Logging severity.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Human readable complementary message. Optional, focus should be
        /// on structured data.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The actual structured payload to be serialized in JSON.
        /// </summary>
        public object Payload { get; set; }

        /// <summary>
        /// Optional exception information, if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Allows framework level enrichment of log entires with additional properties.
        /// </summary>
        public Dictionary<string, object> ContextData { get; } =  new Dictionary<string, object>();
    }
}