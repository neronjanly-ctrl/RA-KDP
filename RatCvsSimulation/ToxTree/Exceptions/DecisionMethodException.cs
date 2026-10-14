using System;

namespace toxTree.exceptions
{
    public class DecisionMethodException : Exception
    {
        private const long serialVersionUID = 2295189243363905691L;

        public DecisionMethodException()
            : base()
        {
        }

        public DecisionMethodException(String message)
            : base(message)
        {
        }

        public DecisionMethodException(Exception cause)
            : base(null, cause)
        {
        }

        public DecisionMethodException(String message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
