using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVScreeningCore.Exception
{
    public class ExceptionAtomicCheckOnProcessForwardImpossible : ApplicationException
    {
        public ExceptionAtomicCheckOnProcessForwardImpossible()
        {
        }

        public ExceptionAtomicCheckOnProcessForwardImpossible(string message)
            : base(message)
        {
        }

        public ExceptionAtomicCheckOnProcessForwardImpossible(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
