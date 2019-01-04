namespace Felfel.Logging
{

    /// <summary>
    /// Primary façade for structured logging.
    /// </summary>
    /// <typeparam name="T">Used to infer the <see cref="ILogger.Context"/>.</typeparam>
    public interface ILogger<T> : ILogger
    {
    }


    /// <summary>
    /// Primary façade for structured logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logger context, which will be part of the serialized data unless explicitly
        /// set through the <see cref="LogEntry.Context"/> property. Identifies messages
        /// that belong together (along with the <see cref="LogEntry.PayloadType"/> at finer
        /// granularity).
        /// </summary>
        string Context { get; }

        /// <summary>
        /// Concatenates the the <see cref="Context"/> and the
        /// <see cref="LogEntry.PayloadType"/> of a log entry
        /// in order to ensure a qualified (and unique) payload type name.
        /// </summary>
        bool PrefixPayloadType { get; set; }

        /// <summary>
        /// Schedules a new log entry for logging. You probably should use one of the convenience
        /// overloads such as <c>Warn</c>, <c>Info</c> or <c>Debug</c> that can be found
        /// in <see cref="LoggerExtensions"/>.
        /// </summary>
        void Log(LogEntry entry);
    }
}