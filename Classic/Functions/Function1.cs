using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Functions
{
    public static class Function1
    {
        public class Error
        {
            public int id { get; set; }
            public string message { get; set; }
        };

        [FunctionName("Validate")]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient client,
            TraceWriter log)
        {
            var body = await req.Content.ReadAsStringAsync();
            var personObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Person>(body);
            log.Info($@"Person request received: 
{body}");
            var orchestrationInstanceId = await client.StartNewAsync(@"Start", personObj);

            return client.CreateCheckStatusResponse(req, orchestrationInstanceId);
        }

        [FunctionName(nameof(Start))]
        public static async Task<IList<Error>> Start([OrchestrationTrigger]DurableOrchestrationContext context)
        {
            var person = context.GetInput<Person>();

            var tasks = new[]
            {
                context.CallActivityAsync<Error>(nameof(CheckFirstName), person),
                context.CallActivityAsync<Error>(nameof(CheckLastName), person)
            };

            var errors = await Task.WhenAll(tasks);

            return errors.Where(e => e != null).ToList();
        }

        [FunctionName(nameof(CheckFirstName))]
        public static async Task<Error> CheckFirstName([ActivityTrigger]DurableActivityContext context, TraceWriter log)
        {
            await Task.Delay(1300);
            var person = context.GetInput<Person>();
            log.Info($@"Message received: {context.InstanceId}");
            if ((person?.Name?.First?.Length > 1) == false)
            {
                var err = new Error { id = 1, message = "First name is null or not longer than 1 character" };
                log.Info($@" - Error found: {err.message}");
                return err;
            }

            log.Info($@" - No error found");
            return null;
        }

        [FunctionName(nameof(CheckLastName))]
        public static async Task<Error> CheckLastName([ActivityTrigger]DurableActivityContext context, TraceWriter log)
        {
            await Task.Delay(1400);

            var person = context.GetInput<Person>();
            if ((person?.Name?.Last?.Length > 1) == false)
            {
                var err = new Error { id = 2, message = "Last name is null or not longer than 1 character" };
                log.Info($@" - Error found: {err.message}");
                return err;
            }

            log.Info($@" - No error found");
            return null;
        }

        //[FunctionName("GetResult")]
        //public static async Task<IActionResult> GetResultAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, [Table(@"checkfirstnameoutput", Connection = @"AzureWebJobsStorage")]CloudTable firstnameoutputTable)
        //{
        //    string targetId = null;

        //    if (req.GetQueryParameterDictionary()?.TryGetValue(@"id", out targetId) == true)
        //    {
        //        var queryResult = await firstnameoutputTable.ExecuteAsync(TableOperation.Retrieve<ErrorTableEntity>(targetId, targetId));
        //        if (queryResult.Result != null)
        //        {
        //            return new OkObjectResult(queryResult.Result);
        //        }

        //        return new NotFoundResult();
        //    }
        //    else
        //    {
        //        return new BadRequestObjectResult(@"'id' parameter is required");
        //    }
        //}

        //[FunctionName("CheckZip")]
        //public static async Task<IActionResult> CheckZip([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        //{
        //    await Task.Delay(1000);
        //    if ((req?.Address?.Zip?.Length == 5) == false)
        //    {
        //        return new OkObjectResult(new Error { id = 6, message = "Zip is null or not 5 digits" });
        //    }

        //    return new NoContentResult();
        //}

        //[FunctionName("CheckState")]
        //public static async Task<IActionResult> CheckState([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        //{
        //    await Task.Delay(1100);
        //    if ((req?.Address?.State?.Length == 2) == false)
        //    {
        //        return new OkObjectResult(new Error { id = 5, message = "State is null or not a 2-letter abbreviation" });
        //    }

        //    return new NoContentResult();
        //}

        //[FunctionName("CheckCity")]
        //public static async Task<IActionResult> CheckCity([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        //{
        //    await Task.Delay(1200);
        //    if (string.IsNullOrEmpty(req?.Address?.City))
        //    {
        //        return new OkObjectResult(new Error { id = 4, message = "City is empty" });
        //    }

        //    return new NoContentResult();
        //}

        //[FunctionName("CheckAddressLines")]
        //public static async Task<IActionResult> CheckAddressLines([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        //{
        //    await Task.Delay(1300);
        //    if (!string.IsNullOrEmpty(req?.Address?.Line2) && string.IsNullOrEmpty(req?.Address?.Line1))
        //    {
        //        return new OkObjectResult(new Error { id = 3, message = "Address line 2 is populated but line 1 is empty" });
        //    }

        //    return new NoContentResult();
        //}
    }
}
