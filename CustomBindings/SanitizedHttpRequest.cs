using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomBindings
{
    public class SanitizedHttpRequest<T>
    {
        public SanitizedHttpRequest()
        {

        }

        public Dictionary<string, string> Query { get; set; }

        public T Body { get; set; }
    }
}
