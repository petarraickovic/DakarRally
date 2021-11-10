using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DakarRally.Models.Exceptions
{
    public class CustomException : Exception
    {
        public CustomException() { }

        public CustomException(string message)
            : base(message) { }

    }
}
