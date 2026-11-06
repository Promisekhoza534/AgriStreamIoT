using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgriStreamFunctions
{
    public class BlobUploadAuditFunction
    {
        private readonly ILogger<BlobUploadAuditFunction> _logger;

        public BlobUploadAuditFunction(
            ILogger<BlobUploadAuditFunction> logger)
        {
            _logger = logger;
        }

        [Function("BlobUploadAudit")]
        public async Task Run(
            [BlobTrigger("drone-images/{name}",
                Connection = "AzureWebJobsStorage")] Stream blob,
            string name)
        {
            string connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage connection string is missing.");

            var tableClient = new TableClient(
                connectionString,
                "SensorReadings");

            await tableClient.CreateIfNotExistsAsync();

            var auditEntry = new TableEntity(
                "DroneUploads",
                Guid.NewGuid().ToString())
            {
                ["EventType"] = "DroneImageUploaded",
                ["FileName"] = name,
                ["ProcessingStatus"] = "Uploaded - Pending Processing",
                ["UploadedAtUtc"] = DateTime.UtcNow,
                ["FileSizeBytes"] = blob.CanSeek ? blob.Length : 0
            };

            await tableClient.AddEntityAsync(auditEntry);

            _logger.LogInformation(
                "Drone image {FileName} uploaded. Audit entry saved to Azure Table Storage.",
                name);
        }
    }
}