using System;

namespace toxTree.exceptions
{
    public class DecisionMethodIOException : DecisionMethodException
    {
        private const long serialVersionUID = -1637646011433572163L;

        public DecisionMethodIOException()
            : base()
        {
        }

        public DecisionMethodIOException(String message)
            : base(message)
        {
        }

        public DecisionMethodIOException(Exception cause)
            : base(cause)
        {
        }

        public DecisionMethodIOException(String message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
