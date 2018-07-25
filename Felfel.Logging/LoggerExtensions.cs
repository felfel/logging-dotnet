using System;

namespace Felfel.Logging
{
    /// <summary>
    /// Convenience overloads to simplify logging.
    /// </summary>
    public static class LoggerExtensions
    {
        private static void WriteEntry(Logger logger, LogLevel level, string payloadType, object data,
            Exception exception, string message)
        {
            LogEntry entry = new LogEntry
            {
                LogLevel = level,
                PayloadType = payloadType,
                Data = data,
                Exception = exception,
                Message = message
            };

            logger.Log(entry);
        }

        public static void Debug(this Logger logger, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Debug, payloadType, data, null, message);
        }

        public static void Debug(this Logger logger, Exception exception, string message = null)
        {
            WriteEntry(logger, LogLevel.Debug, "", null, exception, message);
        }

        public static void Debug(this Logger logger, Exception exception, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Debug, payloadType, data, exception, message);
        }

        public static void Information(this Logger logger, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Info, payloadType, data, null, message);
        }

        public static void Information(this Logger logger, Exception exception, string message = null)
        {
            WriteEntry(logger, LogLevel.Info, "", null, exception, message);
        }

        public static void Information(this Logger logger, Exception exception, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Info, payloadType, data, exception, message);
        }

        public static void Warning(this Logger logger, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Warning, payloadType, data, null, message);
        }

        public static void Warning(this Logger logger, Exception exception, string message = null)
        {
            WriteEntry(logger, LogLevel.Warning, "", null, exception, message);
        }

        public static void Warning(this Logger logger, Exception exception, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Warning, payloadType, data, exception, message);
        }

        public static void Error(this Logger logger, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Error, payloadType, data, null, message);
        }

        public static void Error(this Logger logger, Exception exception, string message = null)
        {
            WriteEntry(logger, LogLevel.Error, "", null, exception, message);
        }

        public static void Error(this Logger logger, Exception exception, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Error, payloadType, data, exception, message);
        }

        public static void Fatal(this Logger logger, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Fatal, payloadType, data, null, message);
        }

        public static void Fatal(this Logger logger, Exception exception, string message = null)
        {
            WriteEntry(logger, LogLevel.Fatal, "", null, exception, message);
        }

        public static void Fatal(this Logger logger, Exception exception, string payloadType, object data, string message = null)
        {
            WriteEntry(logger, LogLevel.Fatal, payloadType, data, exception, message);
        }
    }
}