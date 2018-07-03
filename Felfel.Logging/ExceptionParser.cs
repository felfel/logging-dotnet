using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Felfel.Logging
{
    /// <summary>
    /// A helper class that recursively unwraps specific exceptions
    /// in order to provide a text representation of an exception that
    /// includes important data that would otherwise be hidden if
    /// the exception was just represented using <c>ToString()</c>.<br/>
    /// Note that this class does not parse the <see cref="Exception.Data"/>
    /// dictionary, but only recurses inner / nested exceptions.
    /// </summary>
    /// <remarks>If we feel we want to extend it to provide additional types, refactor
    /// to use strategies for handling different types of exceptions.</remarks>
    public static class ExceptionParser
    {
        /// <summary>
        /// Parses a given <paramref name="exception"/>, and returns a
        /// <see cref="ExceptionData"/> object that contains the root
        /// exception plus hidden exceptions as well as a well-formatted
        /// stack trace.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="exception"/>
        /// is a null reference.</exception>
        public static ExceptionData GetExceptionData(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            var sb = new StringBuilder();
            var ed = new ExceptionData(exception);
            Parse(ed, ed.RootException, sb, 0);
            ed.FormattedException = sb.ToString();
            
            ed.ExceptionHash = CalculateHash(ed);

            return ed;
        }

        /// <summary>
        /// Recursively processes a given <paramref name="exception"/> and
        /// returns a verbose string that reflects exception hierarchies and
        /// stack traces of the submitted root exception and nested (hidden)
        /// exceptions that are not be included in the stack trace.
        /// </summary>
        /// <param name="exception">The root exception to be processed.</param>
        /// <param name="includeRootExceptionStack">Whether to include the
        /// stack trace of the root exception in the returned string. Not needed if
        /// the root exception is being logged anyway.</param>
        /// <returns>A formatted string reflecting the exception hierarchy.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="exception"/>
        /// is a null reference.</exception>
        public static string Parse(Exception exception, bool includeRootExceptionStack)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            ExceptionData ed = GetExceptionData(exception);
            return Print(ed, includeRootExceptionStack);
        }

        /// <summary>
        /// Gets a string representation of the submitted <paramref name="data"/>.
        /// </summary>
        /// <param name="data">Processed exception data.</param>
        /// <param name="includeRootExceptionStack">Whether to include the
        /// stack trace of the root exception in the returned string. Not needed if
        /// the root exception is being logged anyway.</param>
        /// <returns>A formatted string reflecting the exception hierarchy.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/>
        /// is a null reference.</exception>
        public static string Print(ExceptionData data, bool includeRootExceptionStack)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            //get builder message
            var sb = new StringBuilder(data.FormattedException);

            //add full string representation of the root exception
            if (includeRootExceptionStack)
            {
                sb.AppendLine();
                AppendLine(sb, 0, "-");
                sb.AppendLine(data.RootException.ToString());
            }

            if (data.HiddenExceptions.Any())
            {
                //add additional exception stack traces
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine("NESTED / HIDDEN EXCEPTIONS:");
                foreach (var exception in data.HiddenExceptions)
                {
                    sb.AppendLine();
                    AppendLine(sb, 1, exception.ToString());
                    AppendLine(sb, 0, "---");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Main routine that analyzes the currently processed <paramref name="exception"/>.
        /// </summary>
        internal static void Parse(ExceptionData exceptionData, Exception exception, StringBuilder builder, int indent)
        {
            var typeLoadException = exception as ReflectionTypeLoadException;
            var aggregateException = exception as AggregateException;

            exceptionData.AllExceptions.Add(exception);

            if (aggregateException != null)
            {
                ProcessAggregateException(exceptionData, aggregateException, builder, indent);
                //skip recursion to InnerException - covered by the InnerExceptions loop
                return;
            }

            if (typeLoadException != null)
            {
                ProcessTypeLoadException(exceptionData, typeLoadException, builder, indent);
            }
            else
            {
                //default exception header
                AppendFormat(builder, indent, "{0}: {1}", exception.GetType().Name, exception.Message);
                builder.AppendLine();
            }

            //recurse inner exception
            RecurseInnerException(exceptionData, exception,  builder, indent);
        }

        /// <summary>
        /// Triggers recursive parsing of the inner exception of the currently processed exception.
        /// </summary>
        internal static void RecurseInnerException(ExceptionData exceptionData, Exception exception, StringBuilder builder, int indent)
        {
            //recurse inner exception
            if (exception.InnerException != null)
            {
                AppendLine(builder, indent, "- Inner exception:");
                Parse(exceptionData, exception.InnerException, builder, indent + 1);
            }
        }

        /// <summary>
        /// Processes a <see cref="ReflectionTypeLoadException"/> in order to makes sure
        /// type information is included and all <see cref="ReflectionTypeLoadException.LoaderExceptions"/>
        /// are recursively parsed.
        /// </summary>
        internal static void ProcessTypeLoadException(ExceptionData exceptionData, ReflectionTypeLoadException typeLoadException, StringBuilder builder, int indent)
        {
            AppendFormat(builder, indent, "ReflectionTypeLoadException (possibility: MEF composition error): {0}",
                                 typeLoadException.Message);
            builder.AppendLine();
            Append(builder, indent, "- Types: ");
            IEnumerable<string> typeNames = typeLoadException.Types.Select(t => t == null ? "[null]" : t.Name);
            builder.AppendLine(String.Join(",", typeNames));

            AppendLine(builder, indent, "- Loader exceptions:");

            //keep that line above the if statement below, even if there were no Errors
            if (typeLoadException.LoaderExceptions != null)
            {
                foreach (Exception loaderException in typeLoadException.LoaderExceptions)
                {
                    //the loader exceptions array may contain null references!
                    if (loaderException == null) continue;

                    //store hidden exception and recurse
                    exceptionData.HiddenExceptions.Add(loaderException);
                    Parse(exceptionData, loaderException, builder, indent + 1);
                }
            }
        }

        /// <summary>
        /// Processes an <see cref="AggregateException"/> in order to make sure
        /// all inner exceptions are recursively parsed.
        /// </summary>
        internal static void ProcessAggregateException(ExceptionData exceptionData, AggregateException aggregateException, StringBuilder builder, int indent)
        {
            AppendFormat(builder, indent, "AggregateException: {0}", aggregateException.Message);
            builder.AppendLine();
            AppendLine(builder, indent, "- Inner exceptions:"); //keep that line above the if statement below, even if there were no Errors

            //flatten nested aggregate exceptions
            var flattened = aggregateException.Flatten();
            foreach (var innerException in flattened.InnerExceptions)
            {
                Parse(exceptionData, innerException, builder, indent + 1);
            }
        }

        /// <summary>
        /// Indents the current line.
        /// </summary>
        internal static void Indent(StringBuilder builder, int count)
        {
            builder.Append("".PadLeft(count * 4));
        }

        /// <summary>
        /// Appends text with optional indent.
        /// </summary>
        internal static void Append(StringBuilder builder, int indent, string value)
        {
            var paragraphs = value.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
            for (int index = 0; index < paragraphs.Length; index++)
            {
                var paragraph = paragraphs[index];
                Indent(builder, indent);

                builder.Append(paragraph);

                //omit the line break after the last paragraph
                if (index < paragraphs.Length - 1) builder.AppendLine();
            }
        }

        /// <summary>
        /// Appends an optionally indented line.
        /// </summary>
        internal static void AppendLine(StringBuilder builder, int indent, string value)
        {
            Append(builder, indent, value);
            builder.AppendLine();
        }

        /// <summary>
        /// Appends a formatted string with optional indent.
        /// </summary>
        internal static void AppendFormat(StringBuilder builder, int indent, string value, params object[] args)
        {
            var formatted = String.Format(value, args);
            Append(builder, indent, formatted);
        }

        /// <summary>
        /// Gets a hash built based on all exceptions in the submitted <see cref="ExceptionData"/>
        /// object. Hidden exceptions result in multiple dotted hashes, to enable comparison of
        /// other hashes that ultimately result in the same exception.<br/>
        /// Note that if more than 10 exceptions occur, the last hash represents multiple exceptions
        /// (10th and higher).
        /// </summary>
        /// <returns></returns>
        internal static string CalculateHash(ExceptionData exceptionData)
        {
            var list = new List<Exception>(exceptionData.HiddenExceptions);
            list.Insert(0, exceptionData.RootException);

            using (MD5 md5 = MD5.Create())
            {
                List<string> sourceStrings = list.Select(e => GetStackTraces(e)).ToList();

                //TBD: this routine would just create a single (32 character) hash of all exceptions:
                //var unhashed = Encoding.Unicode.GetBytes(String.Concat(sourceStrings));
                //var hash = md5.ComputeHash(unhashed);
                //return String.Concat(hash.Select(b => b.ToString("X2")));

                //if the list contains more than 10 hashes, trim it down by concatenating some
                while (sourceStrings.Count > 10)
                {
                    string st = sourceStrings[9];
                    sourceStrings.RemoveAt(9);
                    sourceStrings[9] = String.Concat(sourceStrings[9], st);
                }


                IEnumerable<string> hashes = from sourceString in sourceStrings
                                             let stack = Encoding.Unicode.GetBytes(sourceString)
                                             let hash = md5.ComputeHash(stack)
                                             select String.Concat(hash.Select(b => b.ToString("X2")));

                //an MD5 hash has 32 characters - trim it down to 8 chars, risk of collisions is basically zero
                //and it's wouldn't even be that much of a problem with proper logging (different context info, too)
                hashes = hashes.Select(h => h.Substring(0, 8));

                return String.Join(".", hashes);
            }
        }

        /// <summary>
        /// Gets all stack traces of a given exception and its inner exceptions
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sb"></param>
        /// <returns></returns>
        private static string GetStackTraces(Exception e, StringBuilder sb = null)
        {
            if (sb == null) sb = new StringBuilder();

            sb.Append(e.GetType().Name);
            string st = e.StackTrace;
            if (String.IsNullOrEmpty(st))
            {
                sb.Append("empty"); //generate something that is hashable...
            }
            else
            {
                sb.Append(st);
            }

            return e.InnerException == null ? sb.ToString() : GetStackTraces(e.InnerException, sb);
        }
    }
}