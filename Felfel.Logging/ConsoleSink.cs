using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Felfel.Logging
{
    /// <summary>
    /// Writes log entries to the console as formatted JSON.
    /// </summary>
    public class ConsoleSink : LogEntrySink
    {
        public ConsoleSink(int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
        {
        }

        protected override Task WriteLogEntry(LogEntryDto entryDto)
        {
            string json = JsonConvert.SerializeObject(entryDto, Formatting.Indented);

            string level = entryDto.Level.ToLower();
            if (level == "error" || level == "fatal")
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(json);
            Console.ResetColor();
            return null; //Task.CompletedTask only av. in .NET Standard
        }
    }
}