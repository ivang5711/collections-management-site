using Collections.Models;

namespace Collections.Services;
/// <summary>
/// Provides methods to interact with Azure blob storage
/// </summary>
public interface IBlobService
{
    /// <summary>
    /// Deletes a blob from Azure storage
    /// </summary>
    /// <param name="blobName">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public Task DeleteBlobAsync(string blobName, string containerName);

    /// <summary>
    /// Get a single blob file from Azure storage
    /// </summary>
    /// <param name="name">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public Task<BlobInfo> GetBlobsAsync(string name, string containerName);

    /// <summary>
    /// Get List of blobs in Azure Storage
    /// </summary>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public Task<IEnumerable<string>> ListBlobsAsync(string containerName);

    /// <summary>
    /// Upload blob file to Azure storage
    /// </summary>
    /// <param name="filePath">path to file</param>
    /// <param name="fileName">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public Task UploadFileBlobAsync(string filePath, string fileName, string containerName);

    /// <summary>
    /// Get Blob Url from Azure storage
    /// </summary>
    /// <param name="name">file name with extension</param>
    /// <param name="containerName">name of container in Azure Storage</param>
    /// <returns></returns>
    public string GetBlobUrl(string name, string containerName);
}