using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Felfel.Logging.UnitTests
{
    [TestClass]
    public class LogEntryParser_when_logging_exception
    {
        private const string errorMessage = "Ouch";

        [TestMethod]
        public void IsException_flag_should_be_true()
        {
            var dto = CreateDto();
            dto.IsException.Should().BeTrue();
        }

        [TestMethod]
        public void Exception_information_should_be_present()
        {
            var dto = CreateDto();
            dto.ExceptionInfo.Should().NotBeNull();
            dto.ExceptionInfo.ErrorMessage.Should().Be(errorMessage);
            dto.ExceptionInfo.ExceptionType.Should().Be(nameof(DivideByZeroException));
            dto.ExceptionInfo.StackTrace.Should().NotBeEmpty();
            dto.ExceptionInfo.ExceptionHash.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Same_exception_with_different_message_should_still_result_in_same_hash()
        {
            var dto1 = CreateDto(message: "aaa");
            var dto2 = CreateDto(message: "bbb");

            dto1.ExceptionInfo.ErrorMessage.Should().Be("aaa");
            dto2.ExceptionInfo.ErrorMessage.Should().Be("bbb");

            dto1.ExceptionInfo.ExceptionHash.Should().Be(dto2.ExceptionInfo.ExceptionHash);
        }


        [TestMethod]
        public void Same_exception_type_only_should_result_in_different_hash()
        {
            var dto1 = CreateDto(false);
            var dto2 = CreateDto(true);

            dto1.ExceptionInfo.ExceptionHash.Should().NotBe(dto2.ExceptionInfo.ExceptionHash);
        }

        private LogEntryDto CreateDto(bool alternativeStackTrace = false, string message = errorMessage)
        {
            var le = new LogEntry { Exception = CreateException(alternativeStackTrace, message) };
            return LogEntryParser.ParseLogEntry(le, "app", "test");
        }

        private Exception CreateException(bool alternativeStackTrace, string message)
        {
            if (alternativeStackTrace)
            {
                try
                {
                    throw new DivideByZeroException(message);
                }
                catch (Exception e)
                {
                    return e;
                }
            }


            try
            {
                throw new DivideByZeroException(message);
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}