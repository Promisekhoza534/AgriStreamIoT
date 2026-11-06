using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AgriStreamFunctions
{
    public class SendIrrigationFunction
    {
        private readonly ILogger<SendIrrigationFunction> _logger;

        public SendIrrigationFunction(ILogger<SendIrrigationFunction> logger)
        {
            _logger = logger;
        }

        [Function("SendIrrigation")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post",
                Route = "irrigation")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                var badResponse = req.CreateResponse(
                    System.Net.HttpStatusCode.BadRequest);

                await badResponse.WriteStringAsync(
                    "Please provide an irrigation command.");

                return badResponse;
            }

            string connectionString =
                System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var queueClient = new QueueClient(
                connectionString, "irrigation-commands");

            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(body);

            var response = req.CreateResponse(
                System.Net.HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                Message = "Irrigation command added to queue.",
                Command = JsonDocument.Parse(body).RootElement
            });

            _logger.LogInformation(
                "Irrigation command sent to the queue.");

            return response;
        }
    }
}