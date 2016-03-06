using System;

namespace CVScreeningCore.Exception
{
    public class ExceptionNotAuthorized : ApplicationException
    {
        public ExceptionNotAuthorized()
        {
        }

        public ExceptionNotAuthorized(string message)
            : base(message)
        {
        }

        public ExceptionNotAuthorized(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
