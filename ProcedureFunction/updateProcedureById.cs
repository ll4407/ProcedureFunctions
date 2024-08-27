using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Threading.Tasks;
using ProcedureFunction;

namespace ProductFunction
{
    public class UpdateProcedureById
    {
        private readonly ILogger<UpdateProcedureById> _logger;

        public UpdateProcedureById(ILogger<UpdateProcedureById> logger)
        {
            _logger = logger;
        }

        [Function("updateProcedureById")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updateProcedureById")]
            HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP PUT trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            try
            {
                // Extract id and procedureId from query parameters
                var queryParameters = req.Url.Query;
                var query = System.Web.HttpUtility.ParseQueryString(queryParameters);

                string id = query["id"] ?? "";
                string procedureId = query["procedureId"] ?? "";

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(procedureId))
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString(JsonSerializer.Serialize(new { message = "ID and ProcedureId are required." }));
                    return response;
                }

                // Parse the request body to get the updated procedure
                var requestBody = await req.ReadAsStringAsync();
                var updatedProcedure = JsonSerializer.Deserialize<Procedure>(requestBody!);

                if (updatedProcedure == null)
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString(JsonSerializer.Serialize(new { message = "Invalid procedure data." }));
                    return response;
                }

                // Create a reference to the CosmosDB instance
                using CosmosClient client = new(
                    connectionString: "INSERT CONNECTION STRING"
                );

                // Access the Cosmos DB container
                Container container = client.GetContainer("Project", "Procedures");

                // Update the item in the container using the provided partition key and ID
                var responseItem = await container.UpsertItemAsync(updatedProcedure, new PartitionKey(procedureId));

                // Return a success message
                response.WriteString(JsonSerializer.Serialize(new { message = "Procedure updated successfully." }));
            }
            catch (CosmosException ex)
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.WriteString(JsonSerializer.Serialize(new { message = ex.Message }));
            }
            catch (Exception ex)
            {
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                response.WriteString(JsonSerializer.Serialize(new { message = ex.Message }));
            }

            return response;
        }
    }
}
