using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Microsoft.Azure.Cosmos;
using ProcedureFunction;
using System.Text.Json;

namespace ProductFunction
{
    public class getProcedureById
    {
        private readonly ILogger<getProcedureById> _logger; // logger, Console.writeline

        public getProcedureById(ILogger<getProcedureById> logger) // dependency injection
        {
            _logger = logger;
        }

        [Function("getProcedureById")] // get procedure by id
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getProcedureById")]
            HttpRequestData req, FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            if (executionContext == null)
            {
                var res = req.CreateResponse(HttpStatusCode.NoContent); // if no execContext, then NoContent, 204
                return res;
            }
            // Create a reference to the cosmosDB instance in our code
            using CosmosClient client = new(
                connectionString: "ENTER CONNECTION STRING HERE"
            );

            var response = req.CreateResponse(HttpStatusCode.OK); // 200
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            try
            {
                // query for a single product in the DB
                var item = await client.GetContainer("Project", "Procedures").ReadItemAsync<Procedure>(
                    executionContext.BindingContext.BindingData["id"]!.ToString(),
                    new PartitionKey(executionContext.BindingContext.BindingData["procedureId"]!.ToString())
                );
                //response.WriteString(item.Resource.ToString() ?? "");
                // Serialize the item to JSON and write to response
                string jsonString = JsonSerializer.Serialize(item.Resource); // Serialize to JSON
                response.WriteString(jsonString);
            }
            catch (Exception ex)
            {
                response.WriteString(ex.Message);
            }
            return response;
        }
    }
}

