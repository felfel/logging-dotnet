using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Felfel.Logging.UnitTests
{
    [TestClass]
    public class LogEntryParser_when_processing_payload
    {
        [TestMethod]
        public void Simple_string_should_be_assigned_to_Message()
        {
            var le = new LogEntry { Payload = "hello world" };
            var dto = LogEntryParser.ParseLogEntry(le, "app", "test");
            dto.Payload.Should().BeNull();
            dto.Message.Should().Be("hello world");
        }

        [TestMethod]
        public void Simple_string_should_be_wrapped_in_anonymous_type_if_message_is_in_use()
        {
            var le = new LogEntry { Payload = "hello world", Message = "foobar"};
            var dto = LogEntryParser.ParseLogEntry(le, "app", "test");

            dto.Message.Should().Be("foobar");
            dto.Payload.Should().NotBeOfType<string>();

            dynamic data = dto.Payload;
            string message = data.Message;
            message.Should().Be("hello world");
        }

        private class Foo { }

        [TestMethod]
        public void Object_payload_should_be_directly_assigned()
        {
            var le = new LogEntry();
            var dto = LogEntryParser.ParseLogEntry(le, "app", "test");
            dto.Payload.Should().BeSameAs(le.Payload);
        }
    }
}