using System;

namespace toxTree.exceptions
{
    public class FilterException : Exception
    {
        private const long serialVersionUID = -4857410117644646316L;

        public FilterException()
        {
        }

        public FilterException(String message)
            : base(message)
        {
        }

        public FilterException(Exception cause)
            : base(null, cause)
        {
        }

        public FilterException(String message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
