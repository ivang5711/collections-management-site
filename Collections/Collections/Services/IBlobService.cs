using Collections.Models;

namespace Collections.Services;

public interface IBlobService
{
    public Task<BlobInfo> GetBlobsAsync(string name, string containerName);

    public Task<IEnumerable<string>> ListBlobsAsync(string containerName);

    public Task UploadFileBlobAsync(string filePath, string fileName, string containerName);

    public Task DeleteBlobAsync(string blobName, string containerName);

    public string GetBlobUrl(string name, string containerName);
}