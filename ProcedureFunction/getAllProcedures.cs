﻿using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.Text.Json;  // Import for JSON serialization

namespace ProcedureFunction
{
    public class getAllProcedures
    {
        private readonly ILogger<getAllProcedures> _logger;

        public getAllProcedures(ILogger<getAllProcedures> logger)
        {
            _logger = logger;
        }

        [Function("getAllProcedures")]  // get all procedures
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getAllProcedures")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (executionContext == null)
            {
                var res = req.CreateResponse(HttpStatusCode.NoContent);  // if no execContext, no content
                return res;
            }

            // Create a reference to the CosmosDB instance in our code
            using CosmosClient client = new(
                connectionString: "INSERT CONNECTION STRING"
            );

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            try
            {
                // Query to get all procedures
                using FeedIterator<Procedure> feed = client.GetContainer("Project", "Procedures").GetItemQueryIterator<Procedure>(
                    queryText: "SELECT * FROM Items"
                );

                var procedures = new List<Procedure>();

                // Iterating over query result pages
                while (feed.HasMoreResults)
                {
                    FeedResponse<Procedure> res = await feed.ReadNextAsync();

                    // Add all items from the current page to the list
                    procedures.AddRange(res);
                }

                // Serialize the list of procedures to JSON and write to response
                response.WriteString(JsonSerializer.Serialize(procedures));
            }
            catch (Exception ex)
            {
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                response.WriteString(ex.Message);
            }

            return response;
        }
    }
}


