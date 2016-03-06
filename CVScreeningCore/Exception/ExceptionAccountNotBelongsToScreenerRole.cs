using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVScreeningCore.Exception
{
    public class ExceptionAccountNotBelongsToScreenerRole : ApplicationException
    {
        public ExceptionAccountNotBelongsToScreenerRole()
        {
        }

        public ExceptionAccountNotBelongsToScreenerRole(string message)
            : base(message)
        {
        }

        public ExceptionAccountNotBelongsToScreenerRole(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
