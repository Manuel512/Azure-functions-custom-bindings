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
        private const string ContentTypeApplicationJson = "application/json";
        public ContentResult OkObjectResult(object value) => new OkResult(value);
        public ContentResult BadRequestObjectResult(object value) => new BadRequestResult(value);
        public ContentResult NotFoundObjectResult(object value) => new NotFoundResult(value);

        protected class BaseContentResult : ContentResult
        {
            public BaseContentResult(object value)
            {
                ContentType = ContentTypeApplicationJson;
                Content = JsonConvert.SerializeObject(value);
            }
        }

        private class OkResult : BaseContentResult
        {
            public OkResult(object value) : base(value)
            {
                StatusCode = (int)HttpStatusCode.OK;
            }
        }

        private class BadRequestResult : BaseContentResult
        {
            public BadRequestResult(object value) : base(value)
            {
                StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private class NotFoundResult : BaseContentResult
        {
            public NotFoundResult(object value) : base(value)
            {
                StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}
