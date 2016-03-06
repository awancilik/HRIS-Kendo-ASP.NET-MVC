using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVScreeningCore.Exception
{
    public class ExceptionAtomicCheckNotQualified : ApplicationException
    {
        public ExceptionAtomicCheckNotQualified()
        {
        }

        public ExceptionAtomicCheckNotQualified(string message)
            : base(message)
        {
        }

        public ExceptionAtomicCheckNotQualified(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
