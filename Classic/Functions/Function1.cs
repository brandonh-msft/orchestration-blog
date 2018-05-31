using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace Functions
{
    public static class Function1
    {
        public class Error
        {
            public int id { get; set; }
            public string message { get; set; }
        };

        public class ErrorTableEntity : TableEntity
        {
            public ErrorTableEntity() { }

            public ErrorTableEntity(Error err, string sessionId)
            {
                this.PartitionKey = this.RowKey = sessionId;

                this.ErrorId = err.id;
                this.ErrorMessage = err.message;
            }

            public int ErrorId { get; set; }

            public string ErrorMessage { get; set; }
        }

        public class QueueMessage
        {
            public string id { get; set; }
            public Person data { get; set; }
        }

        private static HttpClient _client = new HttpClient { BaseAddress = new System.Uri(@"http://localhost:7071/api/") };

        [FunctionName("Validate")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req,
            [ServiceBus(@"function-a", Connection = @"ServiceBusOrchestrationConnection")]out QueueMessage functionAmsg,
            TraceWriter log)
        {
            var id = Guid.NewGuid().ToString();
            log.Info($@"New orchestration started: {id}");
            functionAmsg = new QueueMessage { id = id, data = req };

            return new AcceptedResult($@"/api/GetResult?id={id}", functionAmsg);
        }

        [FunctionName("CheckFirstName")]
        public static void CheckFirstName([ServiceBusTrigger(@"function-a", Connection = @"ServiceBusOrchestrationConnection")]QueueMessage msg,
            [Table(@"checkfirstnameoutput", Connection = @"AzureWebJobsStorage")]out ErrorTableEntity err,
            TraceWriter log)
        {
            log.Info($@"Message received: {msg.id}");
            if ((msg?.data?.Name?.First?.Length > 1) == false)
            {
                err = new ErrorTableEntity(new Error { id = 1, message = "First name is null or not longer than 1 character" }, msg.id);
                log.Info($@" - Error logged: {err.ErrorMessage}");
            }
            else
            {
                err = new ErrorTableEntity(new Error { id = 0 }, msg.id);
                log.Info($@" - NoError logged");
            }
        }

        [FunctionName("GetResult")]
        public static async Task<IActionResult> GetResultAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, [Table(@"checkfirstnameoutput", Connection = @"AzureWebJobsStorage")]CloudTable firstnameoutputTable)
        {
            string targetId = null;

            if (req.GetQueryParameterDictionary()?.TryGetValue(@"id", out targetId) == true)
            {
                var queryResult = await firstnameoutputTable.ExecuteAsync(TableOperation.Retrieve<ErrorTableEntity>(targetId, targetId));
                if (queryResult.Result != null)
                {
                    return new OkObjectResult(queryResult.Result);
                }

                return new NotFoundResult();
            }
            else
            {
                return new BadRequestObjectResult(@"'id' parameter is required");
            }
        }

        [FunctionName("CheckZip")]
        public static async Task<IActionResult> CheckZip([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        {
            await Task.Delay(1000);
            if ((req?.Address?.Zip?.Length == 5) == false)
            {
                return new OkObjectResult(new Error { id = 6, message = "Zip is null or not 5 digits" });
            }

            return new NoContentResult();
        }

        [FunctionName("CheckState")]
        public static async Task<IActionResult> CheckState([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        {
            await Task.Delay(1100);
            if ((req?.Address?.State?.Length == 2) == false)
            {
                return new OkObjectResult(new Error { id = 5, message = "State is null or not a 2-letter abbreviation" });
            }

            return new NoContentResult();
        }

        [FunctionName("CheckCity")]
        public static async Task<IActionResult> CheckCity([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        {
            await Task.Delay(1200);
            if (string.IsNullOrEmpty(req?.Address?.City))
            {
                return new OkObjectResult(new Error { id = 4, message = "City is empty" });
            }

            return new NoContentResult();
        }

        [FunctionName("CheckAddressLines")]
        public static async Task<IActionResult> CheckAddressLines([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        {
            await Task.Delay(1300);
            if (!string.IsNullOrEmpty(req?.Address?.Line2) && string.IsNullOrEmpty(req?.Address?.Line1))
            {
                return new OkObjectResult(new Error { id = 3, message = "Address line 2 is populated but line 1 is empty" });
            }

            return new NoContentResult();
        }

        [FunctionName("CheckLastName")]
        public static async Task<IActionResult> CheckLastName([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        {
            await Task.Delay(1400);
            if ((req?.Name?.Last?.Length > 1) == false)
            {
                return new OkObjectResult(new Error { id = 2, message = "Last name is null or not longer than 1 character" });
            }

            return new NoContentResult();
        }
    }
}
