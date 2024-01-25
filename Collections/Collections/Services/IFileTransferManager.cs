using Microsoft.AspNetCore.Components.Forms;

namespace Collections.Services;

public interface IFileTransferManager
{
    /// <summary>
    /// Deletes the specified file from disk
    /// </summary>
    /// <param name="fileName"></param>
    public void DeleteFileFromDisk(string fileName);

    /// <summary>
    /// Saves IBrowser file to disk
    /// </summary>
    /// <param name="file"></param>
    /// <returns>Returns new random generated name of the file saved to disk
    /// </returns>
    public Task<string> SaveFileToDisk(IBrowserFile file);

    /// <summary>
    /// Set up maximum file size to upload.
    /// Default value is 1024 * 1024 * 5 = 5 MB
    /// </summary>
    /// <param name="size"></param>
    public void SetUpMaxFileSize(int size);

    /// <summary>
    /// Set up name of a folder to be created in the current working directory
    /// </summary>
    /// <param name="directoryName"></param>
    public void SetUpWorkingDirectory(string directoryName);
}