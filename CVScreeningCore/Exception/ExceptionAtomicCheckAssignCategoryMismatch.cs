using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVScreeningCore.Exception
{
    public class ExceptionAtomicCheckAssignCategoryMismatch : ApplicationException
    {
        public ExceptionAtomicCheckAssignCategoryMismatch()
        {
        }

        public ExceptionAtomicCheckAssignCategoryMismatch(string message)
            : base(message)
        {
        }

        public ExceptionAtomicCheckAssignCategoryMismatch(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
