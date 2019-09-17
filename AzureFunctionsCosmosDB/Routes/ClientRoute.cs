using AzureFunctionsCosmosDB.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctionsCosmosDB.Routes
{
    public static class ClientRoute
    {
        /// <summary>
        /// Get all of the clients.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="clients"></param>
        /// <param name="log"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a new client.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="clients"></param>
        /// <param name="log"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Update a clients information.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="clients"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ClientUpdate")]
        public static async Task<IActionResult> ClientUpdate(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "put",
                Route = "clients/{id}"
            )] HttpRequest req,
            [CosmosDB(
                ConnectionStringSetting = "CosmosDB"
            )] DocumentClient documentClient,
            ILogger log,
            string id
        )
        {
            using (StreamReader reader = new StreamReader(req.Body))
            {
                string requestBody = await reader.ReadToEndAsync();

                Client updated = JsonConvert.DeserializeObject<Client>(requestBody);
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("sql1", "Clients");
                Document document = documentClient
                    .CreateDocumentQuery(collectionUri)
                    .Where(t => t.Id == id)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (document == null)
                {
                    return new NotFoundResult();
                }

                if (!string.IsNullOrEmpty(updated.Forename))
                    document.SetPropertyValue("Forename", updated.Forename);
                if (!string.IsNullOrEmpty(updated.Surname))
                    document.SetPropertyValue("Surname", updated.Surname);

                await documentClient.ReplaceDocumentAsync(document);

                return new OkObjectResult((dynamic)document);
            }
        }

        /// <summary>
        /// Delete a single client.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="documentClient"></param>
        /// <param name="log"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [FunctionName("ClientDelete")]
        public static async Task<IActionResult> ClientDelete(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "delete",
                Route = "clients/{id}"
            )] HttpRequest req,
            [CosmosDB(
                ConnectionStringSetting = "CosmosDB"
            )] DocumentClient documentClient,
            ILogger log,
            string id
        )
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("sql1", "Clients");
            Document document = documentClient
                .CreateDocumentQuery(collectionUri)
                .Where(t => t.Id == id)
                .AsEnumerable()
                .FirstOrDefault();

            if (document == null)
            {
                return new NotFoundResult();
            }

            await documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(id) });

            return new OkResult();
        }

        /// <summary>
        /// Get a single client by their Id.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="client"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ClientGet")]
        public static IActionResult ClientGet(
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
            if (client == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(client);
        }
    }
}
