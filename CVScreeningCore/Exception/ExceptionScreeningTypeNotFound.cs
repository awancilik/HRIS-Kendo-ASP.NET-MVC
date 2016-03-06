using System;

namespace CVScreeningCore.Exception
{
    public class ExceptionScreeningTypeNotFound : ApplicationException
    {
        public ExceptionScreeningTypeNotFound()
        {
        }

        public ExceptionScreeningTypeNotFound(string message) : base(message)
        {
        }

        public ExceptionScreeningTypeNotFound(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}