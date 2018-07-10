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
            dto.Exception.Should().NotBeNull();
            dto.Exception.ErrorMessage.Should().Be(errorMessage);
            dto.Exception.ExceptionType.Should().Be(nameof(DivideByZeroException));
            dto.Exception.StackTrace.Should().NotBeEmpty();
            dto.Exception.ExceptionHash.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Same_exception_with_different_message_should_still_result_in_same_hash()
        {
            var dto1 = CreateDto(message: "aaa");
            var dto2 = CreateDto(message: "bbb");

            dto1.Exception.ErrorMessage.Should().Be("aaa");
            dto2.Exception.ErrorMessage.Should().Be("bbb");

            dto1.Exception.ExceptionHash.Should().Be(dto2.Exception.ExceptionHash);
        }


        [TestMethod]
        public void Same_exception_type_only_should_result_in_different_hash()
        {
            var dto1 = CreateDto(false);
            var dto2 = CreateDto(true);

            dto1.Exception.ExceptionHash.Should().NotBe(dto2.Exception.ExceptionHash);
        }

        private LogEntryDto CreateDto(bool alternativeStackTrace = false, string message = errorMessage)
        {
            var le = new LogEntry { Exception = CreateException(alternativeStackTrace, message) };
            return LogEntryParser.ParseLogEntry(le);
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