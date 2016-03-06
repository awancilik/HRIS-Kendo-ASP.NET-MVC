using System;

namespace CVScreeningCore.Exception
{
    public class ExceptionScreeningDeactivated : ApplicationException
    {
        public ExceptionScreeningDeactivated()
        {
        }

        public ExceptionScreeningDeactivated(string message)
            : base(message)
        {
        }

        public ExceptionScreeningDeactivated(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
