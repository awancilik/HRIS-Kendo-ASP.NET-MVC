using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVScreeningCore.Exception
{
    public class ExceptionDispatchingInvalidAtomicCheck : ApplicationException
    {
        public ExceptionDispatchingInvalidAtomicCheck()
        {
        }

        public ExceptionDispatchingInvalidAtomicCheck(string message)
            : base(message)
        {
        }

        public ExceptionDispatchingInvalidAtomicCheck(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
