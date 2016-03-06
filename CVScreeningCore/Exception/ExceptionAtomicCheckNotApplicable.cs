using System;

namespace CVScreeningCore.Exception
{
    public class ExceptionAtomicCheckNotApplicable : ApplicationException
    {
        public ExceptionAtomicCheckNotApplicable()
        {
        }

        public ExceptionAtomicCheckNotApplicable(string message)
            : base(message)
        {
        }

        public ExceptionAtomicCheckNotApplicable(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
