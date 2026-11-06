using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AgriStreamFunctions
{
    public class ProcessIrrigationFunction
    {
        private readonly ILogger<ProcessIrrigationFunction> _logger;

        public ProcessIrrigationFunction(
            ILogger<ProcessIrrigationFunction> logger)
        {
            _logger = logger;
        }

        [Function("ProcessIrrigation")]
        public async Task Run(
            [QueueTrigger("irrigation-commands",
                Connection = "AzureWebJobsStorage")] string message)
        {
            using JsonDocument document = JsonDocument.Parse(message);

            string deviceId = document.RootElement
                .GetProperty("DeviceId").GetString()!;

            string status = document.RootElement
                .GetProperty("Status").GetString()!;

            string connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString")
                ?? throw new InvalidOperationException(
                    "SQL connection string is missing.");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = @"
                UPDATE DeviceRegistry
                SET OperationalStatus = @Status,
                    LastUpdatedUtc = SYSUTCDATETIME()
                WHERE DeviceId = @DeviceId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO DeviceRegistry
                    (DeviceId, OperationalStatus, LastUpdatedUtc)
                    VALUES
                    (@DeviceId, @Status, SYSUTCDATETIME());
                END";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DeviceId", deviceId);
            command.Parameters.AddWithValue("@Status", status);

            await command.ExecuteNonQueryAsync();

            _logger.LogInformation(
                "Device {DeviceId} status updated to {Status}.",
                deviceId, status);
        }
    }
}