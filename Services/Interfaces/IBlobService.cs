using Microsoft.AspNetCore.Http;

namespace Services.Interfaces
{
    public interface IBlobService
    {
        string GetBlobFile(string name);
        Task<IEnumerable<string>> GetAllBlobFiles();
        Task<string> UploadBlobFileAsync(IFormFile file, string bookTitle);
        Task<string> UpdateBlobFileAsync(IFormFile file, string fileNameToUpdate, string newFileName);
        Task<string> RenameBlobFileAsync(string oldFileName, string newFileName);
        Task RemoveBlobFileAsync(string oldFileName);
        Task DeleteBlobFileAsync(string name);
        IEnumerable<string> GetAllBlobsNames();
    }
}
