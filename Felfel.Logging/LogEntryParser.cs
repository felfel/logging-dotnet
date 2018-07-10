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
            var data = entry.Data;
            if (data is string)
            {
                data = new {Message = data};
            }

            return new LogEntryDto
            {
                Timestamp = entry.TimestampOverride,
                Level = entry.LogLevel.ToString(),
                Context = entry.Context ?? "",
                PayloadType = entry.PayloadType ?? "",
                Data = data,
                Exception = exceptionInfo
            };
        }
    }
}