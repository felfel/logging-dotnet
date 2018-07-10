using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Felfel.Logging.UnitTests
{
    [TestClass]
    public class LogEntryParser_when_processing_payload
    {
        [TestMethod]
        public void Simple_string_should_be_wrapped_in_anonymous_type()
        {
            var le = new LogEntry { Data = "hello world" };
            var dto = LogEntryParser.ParseLogEntry(le);
            dto.Data.Should().NotBeOfType<string>();

            dynamic data = dto.Data;
            string message = data.Message;
            message.Should().Be("hello world");
        }

        private class Foo { }

        [TestMethod]
        public void Object_payload_should_be_directly_assigned()
        {
            var le = new LogEntry();
            var dto = LogEntryParser.ParseLogEntry(le);
            dto.Data.Should().BeSameAs(le.Data);
        }
    }
}