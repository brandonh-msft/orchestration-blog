
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Functions
{
    public static class Function1
    {
        private struct Error
        {
            public int id { get; set; }
            public string message { get; set; }
        };

        [FunctionName("Validate")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req, TraceWriter log)
        {
            var errors = new List<Error>();

            if ((req?.Name?.First?.Length > 1) == false)
            {
                errors.Add(new Error { id = 1, message = "First name is null or not longer than 1 character" });
            }

            if ((req?.Name?.Last?.Length > 1) == false)
            {
                errors.Add(new Error { id = 2, message = "Last name is null or not longer than 1 character" });
            }

            if (!string.IsNullOrEmpty(req?.Address?.Line2) && string.IsNullOrEmpty(req?.Address?.Line1))
            {
                errors.Add(new Error { id = 3, message = "Address line 2 is populated but line 1 is empty" });
            }

            if (string.IsNullOrEmpty(req?.Address?.City))
            {
                errors.Add(new Error { id = 4, message = "City is empty" });
            }

            if ((req?.Address?.State?.Length == 2) == false)
            {
                errors.Add(new Error { id = 5, message = "State is null or not a 2-letter abbreviation" });
            }

            if ((req?.Address?.Zip?.Length == 5) == false)
            {
                errors.Add(new Error { id = 6, message = "Zip is null or not 5 digits" });
            }

            return new OkObjectResult(errors);
        }
    }
}
