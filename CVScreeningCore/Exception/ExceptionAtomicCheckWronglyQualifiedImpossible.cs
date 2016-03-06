using System;

namespace CVScreeningCore.Exception
{
    public class ExceptionAtomicCheckWronglyQualifiedImpossible : ApplicationException
    {
        public ExceptionAtomicCheckWronglyQualifiedImpossible()
        {
        }

        public ExceptionAtomicCheckWronglyQualifiedImpossible(string message)
            : base(message)
        {
        }

        public ExceptionAtomicCheckWronglyQualifiedImpossible(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
