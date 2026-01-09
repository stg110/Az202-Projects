using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace StockFinancialFunction
{
    public static class GetStockFinancialInfo
    {
        [FunctionName("GetStockFinancialInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string ticker = req.Query["ticker"];

            if (string.IsNullOrEmpty(ticker))
            {
                return new BadRequestObjectResult("Please pass a stock ticker symbol on the query string or in the request body");
            }

            // Placeholder for actual financial info retrieval
            var financialInfo = new
            {
                Ticker = ticker,
                CompanyName = "Sample Company",
                MarketCap = 1000000000,
                PE_Ratio = 25.5,
                DividendYield = 1.2,
                Revenue = 500000000,
                NetIncome = 100000000
            };

            return new OkObjectResult(financialInfo);
        }
    }
}
