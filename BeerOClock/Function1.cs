using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BeerOClock
{
    public static class Function1
    {
        [FunctionName("HowLongUntilBeerOClock")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var name = req.Query["name"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (IsItBeerOClock())
            {
                return new OkObjectResult("It's Friday and past 4:30 pm. Open up the beer!!");
            }

            var waitingTime = HowLongMustWeWait();

            var responseMessage =
                $"It's {waitingTime.Days} days {waitingTime.Hours} hours {waitingTime.Minutes} minutes and {waitingTime.Seconds} seconds until beer o'clock.";

            return new OkObjectResult(responseMessage);
        }

        private static bool IsItBeerOClock()
        {
            var now = DateTime.Now;
            return now.DayOfWeek == DayOfWeek.Friday && now.TimeOfDay >= TimeSpan.FromHours(16.5);
        }

        private static TimeSpan HowLongMustWeWait()
        {
            var now = DateTime.Now;

            var days = now.DayOfWeek switch
            {
                DayOfWeek.Monday => (DayOfWeek.Friday - now.DayOfWeek),
                DayOfWeek.Tuesday => (DayOfWeek.Friday - now.DayOfWeek),
                DayOfWeek.Wednesday => (DayOfWeek.Friday - now.DayOfWeek),
                DayOfWeek.Thursday => (DayOfWeek.Friday - now.DayOfWeek),
                DayOfWeek.Friday => (DayOfWeek.Friday - now.DayOfWeek),
                DayOfWeek.Saturday => 6,
                DayOfWeek.Sunday => 5,
                _ => throw new ArgumentOutOfRangeException(null, "You're drunk. Go home!!")
            };

            return TimeSpan.FromDays(days) + TimeSpan.FromHours(16.5) - now.TimeOfDay;
        }
    }
}
