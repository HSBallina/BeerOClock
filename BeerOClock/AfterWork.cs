using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BeerOClock
{
    public static class AfterWork
    {
        [FunctionName("HowLongUntilBeerOClock")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                IsItTimeToFail();
            }
            catch (Exception e)
            {
                log.LogError(e, "I failed my duties.");

                var ex = new ApplicationException(e.Message);
                return new ExceptionResult(ex, true);
            }

            string name;

            switch (req.Method.ToLowerInvariant())
            {
                case "post":
                    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    dynamic data = JsonConvert.DeserializeObject(requestBody);
                    name = data?.name;
                    break;
                case "get":
                    name = req.Query["name"];
                    break;
                default:
                    return new BadRequestObjectResult("I don't know what to do with that.");
            }

            var greeting = string.IsNullOrWhiteSpace(name) ? string.Empty : $"Hi {name}! ";

            if (IsItBeerOClock())
            {
                return new OkObjectResult($"{greeting}It's Friday and past 4:30 pm. Open up the beer!!");
            }

            var waitingTime = HowLongMustWeWait();

            var responseMessage =
                $"{greeting}It's {waitingTime.Days} days {waitingTime.Hours} hours {waitingTime.Minutes} minutes and {waitingTime.Seconds} seconds until beer o'clock.";

            return new OkObjectResult(responseMessage);
        }

        private static void IsItTimeToFail()
        {
            var r = new Random(666);

            if (r.Next(1, 100) > 92)
            {
                throw new ApplicationException("I'm too drunk. Try later...");
            }
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
