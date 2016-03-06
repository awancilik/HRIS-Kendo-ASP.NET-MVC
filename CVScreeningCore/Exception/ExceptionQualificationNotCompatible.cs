using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVScreeningCore.Exception
{
    public class ExceptionQualificationNotCompatible : ApplicationException
    {
        public ExceptionQualificationNotCompatible()
        {
        }

        public ExceptionQualificationNotCompatible(string message)
            : base(message)
        {
        }

        public ExceptionQualificationNotCompatible(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }

}
