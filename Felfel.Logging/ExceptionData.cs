using System;
using System.Collections.Generic;

namespace Felfel.Logging
{
    /// <summary>
    /// Wraps data that is being gathered during exception processing.
    /// </summary>
    public class ExceptionData
    {
        /// <summary>
        /// String representation of the processed <see cref="RootException"/>.
        /// </summary>
        public string FormattedException { get;  internal set; }

        /// <summary>
        /// Hidden exceptions to be printed out.
        /// </summary>
        public List<Exception> HiddenExceptions { get; }

        /// <summary>
        /// A flat list of all exceptions, both hidden and the hierarchy of the
        /// root exception.
        /// </summary>
        public List<Exception> AllExceptions { get; }

        /// <summary>
        /// The root exception that is being processed.
        /// </summary>
        public Exception RootException { get; }

        /// <summary>
        /// A hash that was generated based on the contained exception(s).
        /// </summary>
        public string ExceptionHash { get; internal set; }

        /// <summary>
        /// Initializes a new instance base on the submitted
        /// <paramref name="rootException"/>.
        /// </summary>
        internal ExceptionData(Exception rootException)
        {
            RootException = rootException;
            HiddenExceptions = new List<Exception>();
            AllExceptions = new List<Exception>();
        }
    }
}