using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Felfel.Logging.UnitTests")]

namespace Felfel.Logging
{
    internal static class LogEntryParser
    {
        public static LogEntryDto ParseLogEntry(LogEntry entry, string appName, string environment)
        {
            var exception = entry.Exception;
            ExceptionInfo exceptionInfo = null;
            if (exception != null)
            {
                ExceptionData exceptionData = ExceptionParser.GetExceptionData(exception);

                exceptionInfo = new ExceptionInfo
                {
                    ExceptionType = exception.GetType().Name,
                    ErrorMessage = exception.Message,
                    ExceptionHash = exceptionData.ExceptionHash,
                    StackTrace = ExceptionParser.Print(exceptionData, true)
                };
            }

            //transparently assign string values to the message
            //if there is none, otherwise wrap into an object to
            //ensure a logged payload is always a JSON object rather
            //then a scalar.
            //We don't care about other primitives etc. If somebody
            //is stupid enough to log an int, it'll just get serialized
            var payload = entry.Payload;
            var message = entry.Message;
            if (payload is string)
            {
                if (String.IsNullOrEmpty(message))
                {
                    //do not change the original entry - causes issues with multiple sinks
                    message = payload as string;
                    payload = null;
                }
                else
                {
                    payload = new { Message = payload };
                }
            }

            return new LogEntryDto
            {
                Timestamp = entry.TimestampOverride,
                AppName = appName,
                Environment = environment,
                Level = entry.LogLevel.ToString(),
                Context = entry.Context ?? "",
                Message = String.IsNullOrEmpty(message) ? null : message,
                PayloadType = entry.PayloadType,
                Payload = payload,
                ExceptionInfo = exceptionInfo,
                ContextData = entry.ContextData.Count == 0 ? null : entry.ContextData
            };
        }
    }
}