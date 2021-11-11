using DakarRally.Models.ErrorHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DakarRally.Models.Exceptions
{
    public class CustomException : Exception
    {
        public CustomException() { }

        public CustomException(Error error)
            : base(JsonConvert.SerializeObject(error)) { }

    }
}
