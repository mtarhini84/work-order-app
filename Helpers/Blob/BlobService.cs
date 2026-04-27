using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using WorkOrderApp.Helpers.AST;

namespace WorkOrderApp.Helpers
{
    public class BlobService
    {
        private readonly string? connectionString;
        private IAzureTableService _azureTableService;

        public BlobService(IConfiguration configuration, IAzureTableService azureTableService)
        {
            this.connectionString = configuration.GetConnectionString("AST");
            _azureTableService = azureTableService;
        }

        public async Task<bool> CreateBlobAsync(string containerName, string folderPath, string fileName, Stream content, bool overrideExisting = false)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = blobContainerClient.GetBlobClient($"{folderPath}/{fileName}");

                await blobClient.UploadAsync(content, overrideExisting);
            }
            catch (Exception e)
            {
                await HandleLogError(e);
                return false;
            }

            return true;
        }

        public async Task<Stream?> GetBlobAsync(string containerName, string folderPath, string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient($"/{folderPath}/{fileName}");

                var downloadInfo = await blobClient.DownloadAsync();
                return downloadInfo.Value.Content;
            }
            catch (Exception e)
            {
                await HandleLogError(e);
            }
            return null;

        }

        public async Task<string?> GetBlobLinkAsync(string containerName, string folderPath, string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient($"{folderPath}/{fileName}");

                return blobClient.Uri.ToString();
            }
            catch (Exception e)
            {
                await HandleLogError(e);
            }

            return null;
        }

        public async Task<bool> DeleteBlobAsync(string containerName, string folderPath, string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient($"{folderPath}/{fileName}");

                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception e)
            {
                await HandleLogError(e);
                return false;
            }
        }

        private async Task HandleLogError(Exception e)
        {
            await _azureTableService.CreateEntity("RentalFailedLogs", "blob", Guid.NewGuid().ToString(),
            new Dictionary<string, object>()
            {
                    {"Error", e.Message },
            });
        }
    }
}
