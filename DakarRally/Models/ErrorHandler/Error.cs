using DakarRally.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DakarRally.Models.ErrorHandler
{
    public class Error
    {
        public string Message { get; set; }

        public int StatusCode { get; set; }




        public Error CreateBadRequestError(string message)
        {
            Message = message;
            StatusCode = 400;
            return this;
        }

        public ObjectResult ReturnHttpResponse()
        {
            ObjectResult result = new(this) { StatusCode = StatusCode };
            return result;
        }

        public CustomException ThrowException()
        {
            throw new CustomException(JsonConvert.SerializeObject(this));
        }


        public ObjectResult ProcessExceptionAndReturnResponse(Exception ee)
        {
            if(ee.GetType() == typeof(CustomException))
            {
                Error err = JsonConvert.DeserializeObject<Error>(ee.Message);
                return err.ReturnHttpResponse();
            }
            else 
            {
                return new ObjectResult(ee.Message) { StatusCode = 500 };
            }



        }

    }
}
