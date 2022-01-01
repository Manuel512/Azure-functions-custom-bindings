using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CustomBindings
{
    public interface IAntiXSSActionResult
    {
        ContentResult BadRequestObjectResult(object value);
        ContentResult NotFoundObjectResult(object value);
        ContentResult OkObjectResult(object value);
    }

    public class AntiXSSActionResult : IAntiXSSActionResult
    {
        public ContentResult OkObjectResult(object value) => new OkResult(value);
        public ContentResult BadRequestObjectResult(object value) => new BadRequestResult(value);
        public ContentResult NotFoundObjectResult(object value) => new NotFoundResult(value);

        private class OkResult : ContentResult
        {
            private const string ContentTypeApplicationJson = "application/json";

            public OkResult(object value)
            {
                ContentType = ContentTypeApplicationJson;
                Content = JsonConvert.SerializeObject(value);
                StatusCode = (int)HttpStatusCode.OK;
            }
        }

        private class BadRequestResult : ContentResult
        {
            private const string ContentTypeApplicationJson = "application/json";

            public BadRequestResult(object value)
            {
                ContentType = ContentTypeApplicationJson;
                Content = JsonConvert.SerializeObject(value);
                StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private class NotFoundResult : ContentResult
        {
            private const string ContentTypeApplicationJson = "application/json";

            public NotFoundResult(object value)
            {
                ContentType = ContentTypeApplicationJson;
                Content = JsonConvert.SerializeObject(value);
                StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}
