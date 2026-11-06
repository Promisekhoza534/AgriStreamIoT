using Azure;
using Azure.Data.Tables;

namespace AgriStreamIoT.Services
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _tableClient = new TableClient(
                connectionString,
                "SensorReadings");
        }

        public async Task AddSensorReadingAsync(
            string deviceId,
            double temperature,
            double humidity,
            double soilMoisture)
        {
            await _tableClient.CreateIfNotExistsAsync();

            var reading = new TableEntity(
                "Farm001",
                Guid.NewGuid().ToString())
            {
                ["DeviceId"] = deviceId,
                ["Temperature"] = temperature,
                ["Humidity"] = humidity,
                ["SoilMoisture"] = soilMoisture
            };

            await _tableClient.AddEntityAsync(reading);
        }

        public async Task<List<TableEntity>> GetSensorReadingsAsync()
        {
            await _tableClient.CreateIfNotExistsAsync();

            return _tableClient.Query<TableEntity>().ToList();
        }
    }
}