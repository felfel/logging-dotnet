using System;

namespace Felfel.Logging
{
    /// <summary>
    /// Convenience overloads to simplify logging.
    /// </summary>
    public static class LoggerExtensions
    {
        private static void WriteEntry(Logger logger, LogLevel level, string payloadType, object data,
            Exception exception)
        {
            LogEntry entry = new LogEntry
            {
                LogLevel = level,
                PayloadType = payloadType,
                Data = data,
                Exception = exception
            };

            logger.Log(entry);
        }

        public static void Debug(this Logger logger, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Debug, payloadType, data, null);
        }

        public static void Debug(this Logger logger, Exception exception)
        {
            WriteEntry(logger, LogLevel.Debug, "", null, exception);
        }

        public static void Debug(this Logger logger, Exception exception, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Debug, payloadType, data, exception);
        }

        public static void Information(this Logger logger, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Info, payloadType, data, null);
        }

        public static void Information(this Logger logger, Exception exception)
        {
            WriteEntry(logger, LogLevel.Info, "", null, exception);
        }

        public static void Information(this Logger logger, Exception exception, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Info, payloadType, data, exception);
        }

        public static void Warning(this Logger logger, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Warning, payloadType, data, null);
        }

        public static void Warning(this Logger logger, Exception exception)
        {
            WriteEntry(logger, LogLevel.Warning, "", null, exception);
        }

        public static void Warning(this Logger logger, Exception exception, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Warning, payloadType, data, exception);
        }

        public static void Error(this Logger logger, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Error, payloadType, data, null);
        }

        public static void Error(this Logger logger, Exception exception)
        {
            WriteEntry(logger, LogLevel.Error, "", null, exception);
        }

        public static void Error(this Logger logger, Exception exception, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Error, payloadType, data, exception);
        }

        public static void Fatal(this Logger logger, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Fatal, payloadType, data, null);
        }

        public static void Fatal(this Logger logger, Exception exception)
        {
            WriteEntry(logger, LogLevel.Fatal, "", null, exception);
        }

        public static void Fatal(this Logger logger, Exception exception, string payloadType, object data)
        {
            WriteEntry(logger, LogLevel.Fatal, payloadType, data, exception);
        }
    }
}