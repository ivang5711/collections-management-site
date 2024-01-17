using Microsoft.AspNetCore.Components.Forms;

namespace Collections.Services
{
    public interface IFileTransferManager
    {
        public Task<string> SaveFileToDisk(IBrowserFile file);

        public void DeleteFileFromDisk(string fileName);

        public void SetUpMaxFileSize(int size);

        public void SetUpWorkingDirectory(string directoryName);
    }
}