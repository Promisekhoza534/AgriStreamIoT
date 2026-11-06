using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AgriStreamFunctions
{
    public class GetTelemetryFunction
    {
        private readonly ILogger<GetTelemetryFunction> _logger;

        public GetTelemetryFunction(ILogger<GetTelemetryFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetTelemetry")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get",
                Route = "telemetry")] HttpRequestData req)
        {
            string connectionString =
                System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var tableClient = new TableClient(
                connectionString, "SensorReadings");

            var readings = tableClient.Query<TableEntity>()
                .Where(e => e.PartitionKey != "DroneUploads")
                .OrderByDescending(e => e.Timestamp)
                .Take(20)
                .ToList();

            var response = req.CreateResponse(
                System.Net.HttpStatusCode.OK);

            await response.WriteAsJsonAsync(readings);

            _logger.LogInformation(
                "Retrieved {Count} telemetry records.", readings.Count);

            return response;
        }
    }
}