using Microsoft.AspNetCore.Components.Forms;

namespace Collections.Services;

public class FileTransferManager : IFileTransferManager
{
    private string _directoryPath = string.Empty;
    private int maxFileSize = 1024 * 1024 * 5;

    /// <summary>
    /// Deletes the specified file from disk
    /// </summary>
    /// <param name="fileName"></param>
    public void DeleteFileFromDisk(string fileName)
    {
        string path = Path.Combine(_directoryPath, fileName);
        File.Delete(path);
    }

    /// <summary>
    /// Saves IBrowser file to disk
    /// </summary>
    /// <param name="file"></param>
    /// <returns>Returns new random generated name of the file saved to disk</returns>
    public async Task<string> SaveFileToDisk(IBrowserFile file)
    {
        Directory.CreateDirectory(_directoryPath);
        string newFileName = GetNewRandomFileNamePreserveExtension(file.Name);
        string path = Path.Combine(_directoryPath, newFileName);
        using FileStream fs = new(path, FileMode.Create);
        await file.OpenReadStream(maxFileSize).CopyToAsync(fs);
        return newFileName;
    }



    /// <summary>
    /// Set up maximum file size to upload.
    /// Default value is 1024 * 1024 * 5 = 5 MB
    /// </summary>
    /// <param name="size"></param>
    public void SetUpMaxFileSize(int size)
    {
        if (size > 0)
        {
            maxFileSize = size;
        }
    }

    /// <summary>
    /// Set up name of a folder to be created in the current working directory
    /// </summary>
    /// <param name="directoryName"></param>
    public void SetUpWorkingDirectory(string directoryName)
    {
        var basePath = Directory.GetCurrentDirectory();
        _directoryPath = Path.Combine(basePath, directoryName);
    }

    private string GetNewRandomFileNamePreserveExtension(string oldName)
    {
        return Path.ChangeExtension(Path.GetRandomFileName(),
                Path.GetExtension(oldName));
    }
}