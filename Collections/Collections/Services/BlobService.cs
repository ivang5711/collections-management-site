using Azure.Storage.Blobs;
using Collections.Extensions;
using Collections.Models;

namespace Collections.Services;

public class BlobService(BlobServiceClient blobServiceClient) : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    public async Task DeleteBlobAsync(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteAsync();
    }

    public async Task<BlobInfo> GetBlobsAsync(string name, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(name);
        var blobDownloadInfo = await blobClient.DownloadAsync();
        return new BlobInfo(blobDownloadInfo.Value.Content, blobDownloadInfo.Value.ContentType);
    }

    public async Task<IEnumerable<string>> ListBlobsAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var items = new List<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            items.Add(blobItem.Name);
        }

        return items;
    }

    public async Task UploadFileBlobAsync(string filePath, string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(filePath, new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = filePath.GetContentType() });
    }

    public string GetBlobUrl(string name, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(name);
        var blobUrl = blobClient.Uri.AbsoluteUri;

        return blobUrl;
    }
}