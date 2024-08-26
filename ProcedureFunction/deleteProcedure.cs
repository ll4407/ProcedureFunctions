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
    public class deleteProcedure
    {
        private readonly ILogger<deleteProcedure> _logger;

        public deleteProcedure(ILogger<deleteProcedure> logger)
        {
            _logger = logger;
        }

        [Function("deleteProcedure")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "deleteProcedure")]
            HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP DELETE trigger function processed a request.");

            // Extract id and partitionKey from query parameters
            var queryParameters = req.Url.Query;
            var query = System.Web.HttpUtility.ParseQueryString(queryParameters);

            string id = query["id"] ?? "";
            string partitionKey = query["procedureId"] ?? "";

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            try
            {
                // Validate input
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(partitionKey))
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString(JsonSerializer.Serialize(new { message = "ID and PartitionKey are required." }));
                    return response;
                }

                // Create a reference to the CosmosDB instance
                using CosmosClient client = new(
                    connectionString: "AccountEndpoint=https://jlm-dekron-dental.documents.azure.com:443/;AccountKey=RlhLmrxt0Bd8bd32GGuZIbh1ycG4kmAyHKWAWMFpPkd0RfI3v5mMagV35Las5kBjQjpvCJmUtVuhACDbdtGzdw==;"
                );

                // Access the Cosmos DB container
                Container container = client.GetContainer("Project", "Procedures");

                // Delete the item from the container using the provided partition key and ID
                ItemResponse<Procedure> deleteResponse = await container.DeleteItemAsync<Procedure>(id, new PartitionKey(partitionKey));

                // If the delete operation was successful, return a success message
                response.WriteString(JsonSerializer.Serialize(new { message = "Procedure deleted successfully." }));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Handle case where the item is not found
                response = req.CreateResponse(HttpStatusCode.NotFound);
                response.WriteString(JsonSerializer.Serialize(new { message = "Procedure not found." }));
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                response.WriteString(JsonSerializer.Serialize(new { message = ex.Message }));
            }

            return response;
        }
    }
}
