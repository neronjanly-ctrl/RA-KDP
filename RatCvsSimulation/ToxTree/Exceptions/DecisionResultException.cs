using System;

namespace toxTree.exceptions
{
    public class DecisionResultException : Exception
    {
        private const long serialVersionUID = -4787147813217836285L;

        public DecisionResultException()
            : base()
        {
        }

        public DecisionResultException(String message)
            : base(message)
        {
        }

        public DecisionResultException(Exception cause)
            : base(null, cause)
        {
        }

        public DecisionResultException(String message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
