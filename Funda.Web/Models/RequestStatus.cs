using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Funda.Web.Models
{
    public class RequestStatus
    {
        public HttpStatusCode errorCode;
        public string message;

        public void set(HttpStatusCode error, string msg) { errorCode = error; message = msg; }

        public override string ToString()
        {
            return $" Erro Code: {errorCode} - {message}";
        }



    }
}