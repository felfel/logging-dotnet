using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Felfel.Logging.UnitTests")]

namespace Felfel.Logging
{
    internal static class LogEntryParser
    {
        public static LogEntryDto ParseLogEntry(LogEntry entry)
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

            //transparently wrap string values into an object to
            //ensure a logged payload is always a JSON object rather
            //then a scalar.
            //We don't care about other primitives etc. If somebody
            //is stupid enough to log an int, it'll just get serialized
            var data = entry.Payload;
            if (data is string)
            {
                data = new { Message = data };
            }

            var payloadType = entry.PayloadType;
            if (entry.Payload == null)
            {
                //omit the payload type if there is no payload in the first place
                payloadType = null;
            }

            return new LogEntryDto
            {
                Timestamp = entry.TimestampOverride,
                Level = entry.LogLevel.ToString(),
                Context = entry.Context ?? "",
                Message = String.IsNullOrEmpty(entry.Message) ? null : entry.Message,
                PayloadType = payloadType,
                Payload = data,
                ExceptionInfo = exceptionInfo
            };
        }
    }
}