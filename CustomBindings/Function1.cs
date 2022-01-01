using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using CustomBindings.Bindings;

namespace CustomBindings
{
    public class Function1
    {
        private readonly IAntiXSSActionResult _antiXSSActionResult;

        public Function1(IAntiXSSActionResult antiXSSObjectResult)
        {
            _antiXSSActionResult = antiXSSObjectResult;
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [AntiXSSHttpRequest] SanitizedHttpRequest<Model<SubModel>> httpReq,
            //[CosmosDB(
            //databaseName: "mam-integration",
            //collectionName: "draft-assets",
            //ConnectionStringSetting = "CosmosDB",
            //SqlQuery = "select * from c where c.jobId = 'a8d2ec3f-6bbf-4202-bfb6-6ad57101ab06'")] IEnumerable<object> items,
            ILogger log)
        {
            log.LogInformation(JsonConvert.SerializeObject(httpReq.Body));
            log.LogInformation("-------------------------------------------");

            return new OkObjectResult(httpReq);
        }

        [FunctionName("Function2")]
        public async Task<IActionResult> Run2(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return _antiXSSActionResult.OkObjectResult(new SubModel { Name = "<script>alert(\"xss\")</script>testname" });
        }
    }

    public class Model<T>
    {
        public Guid JobId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public T SubModel { get; set; }
    }

    public class SubModel
    {
        public string Name { get; set; }
    }
}
