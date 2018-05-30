
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Functions
{
    public static class Function1
    {
        private class Error
        {
            public int id { get; set; }
            public string message { get; set; }
        };

        [FunctionName("Validate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]Person req, TraceWriter log)
        {
            var errors = new List<Error>();

            // Fan Out
            var checks = new List<Task<Error>>
{
    CheckFirstName(req, errors),
    CheckLastName(req, errors),
    CheckAddressLines(req, errors),
    CheckCity(req, errors),
    CheckState(req, errors),
    CheckZip(req, errors)
};

            // wait for all checks to complete (Fan In)
            await Task.WhenAll(checks);

            // Add any non-null values to our errors list
            errors.AddRange(checks.Select(t => t.Result).Where(r => r != null));

            return new OkObjectResult(errors);
        }

        private static async Task<Error> CheckZip(Person req, List<Error> errors)
        {
            await Task.Delay(1000);
            if ((req?.Address?.Zip?.Length == 5) == false)
            {
                return new Error { id = 6, message = "Zip is null or not 5 digits" };
            }

            return null;
        }

        private static async Task<Error> CheckState(Person req, List<Error> errors)
        {
            await Task.Delay(1100);
            if ((req?.Address?.State?.Length == 2) == false)
            {
                return new Error { id = 5, message = "State is null or not a 2-letter abbreviation" };
            }

            return null;
        }

        private static async Task<Error> CheckCity(Person req, List<Error> errors)
        {
            await Task.Delay(1200);
            if (string.IsNullOrEmpty(req?.Address?.City))
            {
                return new Error { id = 4, message = "City is empty" };
            }

            return null;
        }

        private static async Task<Error> CheckAddressLines(Person req, List<Error> errors)
        {
            await Task.Delay(1300);
            if (!string.IsNullOrEmpty(req?.Address?.Line2) && string.IsNullOrEmpty(req?.Address?.Line1))
            {
                return new Error { id = 3, message = "Address line 2 is populated but line 1 is empty" };
            }

            return null;
        }

        private static async Task<Error> CheckLastName(Person req, List<Error> errors)
        {
            await Task.Delay(1400);
            if ((req?.Name?.Last?.Length > 1) == false)
            {
                return new Error { id = 2, message = "Last name is null or not longer than 1 character" };
            }

            return null;
        }

        private static async Task<Error> CheckFirstName(Person req, List<Error> errors)
        {
            await Task.Delay(1500);
            if ((req?.Name?.First?.Length > 1) == false)
            {
                return new Error { id = 1, message = "First name is null or not longer than 1 character" };
            }

            return null;
        }
    }
}
