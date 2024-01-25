using Azure.Storage.Blobs;
using Collections.Extensions;
using Collections.Models;

namespace Collections.Services;
/// <summary>
/// Provides methods to interact with Azure blob storage
/// </summary>
/// <param name="blobServiceClient"></param>
public class BlobService(BlobServiceClient blobServiceClient) : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    /// <summary>
    /// Deletes a blob from Azure storage
    /// </summary>
    /// <param name="blobName">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public async Task DeleteBlobAsync(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteAsync();
    }


    /// <summary>
    /// Get a single blob file from Azure storage
    /// </summary>
    /// <param name="name">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public async Task<BlobInfo> GetBlobsAsync(string name, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(name);
        var blobDownloadInfo = await blobClient.DownloadAsync();
        return new BlobInfo(blobDownloadInfo.Value.Content, blobDownloadInfo.Value.ContentType);
    }


    /// <summary>
    /// Get List of blobs in Azure Storage
    /// </summary>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
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

    /// <summary>
    /// Upload blob file to Azure storage
    /// </summary>
    /// <param name="filePath">path to file</param>
    /// <param name="fileName">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public async Task UploadFileBlobAsync(string filePath, string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(filePath, new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = filePath.GetContentType() });
    }

    /// <summary>
    /// Get Blob Url from Azure storage
    /// </summary>
    /// <param name="name">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public string GetBlobUrl(string name, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(name);
        var blobUrl = blobClient.Uri.AbsoluteUri;

        return blobUrl;
    }
}