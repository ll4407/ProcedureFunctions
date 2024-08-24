using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Microsoft.Azure.Cosmos;
using ProcedureFunction;
using System.Text.Json;

namespace ProductFunction
{
    public class addProcedure
    {
        private readonly ILogger<addProcedure> _logger; // logger, Console.writeline

        public addProcedure(ILogger<addProcedure> logger) // dependency injection
        {
            _logger = logger;
        }

        [Function("addProcedure")] // function for POST operation
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addProcedure")]
            HttpRequestData req, FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP POST trigger function processed a request.");
            if (executionContext == null)
            {
                //var res = req.CreateResponse(HttpStatusCode.NoContent); // if no execContext, then NoContent, 204
                //return res;
            }
            // Create a reference to the cosmosDB instance in our code
            using CosmosClient client = new(
                connectionString: "ENTER CONNECTION STRING HERE"
            );

            var response = req.CreateResponse(HttpStatusCode.OK); // 200
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            try
            {
                // Read the request body to get the Procedure data
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Procedure? newProcedure = JsonSerializer.Deserialize<Procedure>(requestBody);

                if (newProcedure == null)
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString("Invalid request payload.");
                    return response;
                }

                // Insert the new Procedure into the CosmosDB container
                Container container = client.GetContainer("Project", "Procedures");
                ItemResponse<Procedure> createResponse = await container.CreateItemAsync(newProcedure, new PartitionKey(newProcedure.procedureId));

                // Serialize the created item to JSON and write to response
                string jsonString = JsonSerializer.Serialize(createResponse.Resource);
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

