using AzureFunctionsCosmosDB.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctionsCosmosDB.Routes
{
    public static class ClientRoute
    {
        [FunctionName("Clients")]
        public static IActionResult Clients(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "clients"
            )] HttpRequest req,
            [CosmosDB(
                "sql1",
                "Clients",
                ConnectionStringSetting = "CosmosDB"
            )] IEnumerable<Client> clients,
            ILogger log
        )
        {
            return new OkObjectResult(clients);
        }

        [FunctionName("ClientCreate")]
        public static async Task<IActionResult> ClientCreate(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "clients"
            )] HttpRequest req,
            [CosmosDB(
                "sql1",
                "Clients",
                ConnectionStringSetting = "CosmosDB"
            )] IAsyncCollector<Client> clients,
            ILogger log
        )
        {
            using (StreamReader reader = new StreamReader(req.Body))
            {
                string requestBody = await reader.ReadToEndAsync();

                Client input = JsonConvert.DeserializeObject<Client>(requestBody);
                Client client = new Client()
                {
                    Forename = input.Forename,
                    Surname = input.Surname
                };

                await clients.AddAsync(client);

                return new OkObjectResult(client);
            }
        }

        [FunctionName("ClientById")]
        public static IActionResult ClientById(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "clients/{id}"
            )] HttpRequest req,
            [CosmosDB(
                "sql1",
                "Clients",
                ConnectionStringSetting = "CosmosDB",
                Id = "{id}",
                PartitionKey = "{id}"
            )] Client client,
            ILogger log
        )
        {
            return new OkObjectResult(client);
        }
    }
}
