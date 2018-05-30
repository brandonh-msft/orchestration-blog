
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Functions
{
    public static class Function1
    {
        private class Error
        {
            public int id { get; set; }
            public string message { get; set; }
        };

        private static HttpClient _client = new HttpClient { BaseAddress = new System.Uri(@"http://localhost:7071/api/") };

        [FunctionName("Validate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req, TraceWriter log)
        {
            var errors = new List<Error>();

            var personPostContent = new StringContent(JsonConvert.SerializeObject(req), Encoding.Default, @"application/json");
            // Fan Out
            var checks = new List<Task<HttpResponseMessage>>
            {
                _client.PostAsync(@"CheckFirstName", personPostContent),
                _client.PostAsync(@"CheckLastName", personPostContent),
                _client.PostAsync(@"CheckAddressLines", personPostContent),
                _client.PostAsync(@"CheckCity", personPostContent),
                _client.PostAsync(@"CheckState", personPostContent),
                _client.PostAsync(@"CheckZip", personPostContent)
            };

            // wait for all checks to complete (Fan In)
            var responses = await Task.WhenAll(checks);

            // Add any non-null values to our errors list
            foreach (var response in responses)
            {
                var err = await response.Content.ReadAsAsync<Error>();
                if (err != null)
                {
                    errors.Add(err);
                }
            }

            return new OkObjectResult(errors);
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

        [FunctionName("CheckFirstName")]
        public static async Task<IActionResult> CheckFirstName([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req)
        {
            await Task.Delay(1500);
            if ((req?.Name?.First?.Length > 1) == false)
            {
                return new OkObjectResult(new Error { id = 1, message = "First name is null or not longer than 1 character" });
            }

            return new NoContentResult();
        }
    }
}
