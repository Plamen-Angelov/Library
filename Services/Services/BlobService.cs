using System.Web;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using log4net;
using Services.Interfaces;
using static Common.ExceptionMessages;
using static Common.GlobalConstants;

namespace Services.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly ILog log = LogManager.GetLogger(typeof(BlobService));

        public BlobService(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadBlobFileAsync(IFormFile file, string bookTitle)
        {
            var fileName = bookTitle;
            var fileExtension = Path.GetExtension(file.FileName);
            var allFileName = String.Concat(fileName, fileExtension);

            if (fileExtension != ".png" &&
                fileExtension != ".jpg" &&
                fileExtension != ".jpeg" &&
                fileExtension != ".PNG" &&
                fileExtension != ".JPG" &&
                fileExtension != ".JPEG")
            {
                log.Error($"Upload method throws exception {FILE_NOT_CORRECT_FORMAT}");
                throw new ArgumentException(FILE_NOT_CORRECT_FORMAT);
            }

            if (file.Length > 512 * 1024)
            {
                log.Error($"Upload method throws exception {FILE_OVER_SIZE}");
                throw new ArgumentException(FILE_OVER_SIZE);
            }

            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);
            var blobClient = containerClient.GetBlobClient(allFileName);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);

            if (result == null)
            {
                log.Error($"Upload method throws exception {FILE_UPLOAD_FAILED}");
                throw new ArgumentException(FILE_UPLOAD_FAILED);
            }

            log.Info("File uploaded successfully.");
            return blobClient.Uri.AbsoluteUri;
        }

        public string GetBlobFile(string name)
        {
            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);
            var blobClient = containerClient.GetBlobClient(name);

            var blobs = GetAllBlobsNames();

            if (!blobs.Any())
            {
                log.Error($"GetBlobFile method throws exception {BLOBSTORAGE_IS_EMPTY}");
                throw new ArgumentException(BLOBSTORAGE_IS_EMPTY);
            }

            if (!blobs.Contains(blobClient.Name))
            {
                log.Error($"GetBlobFile method throws exception {BLOBFILE_NOT_EXIST}");
                throw new ArgumentException(BLOBFILE_NOT_EXIST);
            }

            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<string> UpdateBlobFileAsync(IFormFile file, string fileNameToUpdate, string newFileName)
        {
            var fileName = newFileName;
            var fileExtension = Path.GetExtension(file.FileName);
            var createdFileName = String.Concat(fileName, fileExtension);

            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);

            fileNameToUpdate = HttpUtility.UrlDecode(fileNameToUpdate);

            fileNameToUpdate = fileNameToUpdate.Replace(containerClient.Uri.AbsoluteUri + '/', String.Empty);

            var oldBlobClient = containerClient.GetBlobClient(fileNameToUpdate);
            var newBlobClient = containerClient.GetBlobClient(createdFileName);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            if (!oldBlobClient.Exists())
            {
                log.Error($"UpdateBlobFile method throws exception {BLOBFILE_NOT_EXIST}");
                throw new ArgumentException(BLOBFILE_NOT_EXIST);
            }

            await oldBlobClient.DeleteAsync();

            var result = await newBlobClient.UploadAsync(file.OpenReadStream(), httpHeaders);

            if (result == null)
            {
                log.Error($"UpdateBlobFile method throws exception {FILE_UPLOAD_FAILED}");
                throw new ArgumentException(FILE_UPLOAD_FAILED);
            }

            log.Info("Update of blob file was successful");

            return newBlobClient.Uri.AbsoluteUri;
        }

        public async Task<string> RenameBlobFileAsync(string oldFileName, string newFileName)
        {
            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);

            var oldUri = new Uri(oldFileName);
            var oldName = System.IO.Path.GetFileName(oldUri.LocalPath);
            var oldExtension = System.IO.Path.GetExtension(oldUri.LocalPath);

            var createdFileName = String.Concat(newFileName, oldExtension);

            var oldBlobClient = containerClient.GetBlobClient(oldName);
            var newBlobClient = containerClient.GetBlobClient(createdFileName);

            if (!oldBlobClient.Exists())
            {
                log.Error($"RenameBlobFileAsync method throws exception {BLOBFILE_NOT_EXIST}");
                throw new ArgumentException(BLOBFILE_NOT_EXIST);
            }

            var result = await newBlobClient.StartCopyFromUriAsync(oldUri);

            if (result == null)
            {
                log.Error($"RenameBlobFileAsync method throws exception {FILE_UPLOAD_FAILED}");
                throw new ArgumentException(FILE_UPLOAD_FAILED);
            }

            await oldBlobClient.DeleteAsync();
            log.Info("Blob file was renamed successful");

            return newBlobClient.Uri.AbsoluteUri;
        }

        public async Task RemoveBlobFileAsync(string oldFileName)
        {
            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);

            var oldUri = new Uri(oldFileName);
            var oldName = System.IO.Path.GetFileName(oldUri.LocalPath);

            oldName = oldName.Replace("%20", " ");

            var oldBlobClient = containerClient.GetBlobClient(oldName);

            if (!oldBlobClient.Exists())
            {
                log.Error($"RemoveBlobFileAsync method throws exception {BLOBFILE_NOT_EXIST}");
                throw new ArgumentException(BLOBFILE_NOT_EXIST);
            }

            await oldBlobClient.DeleteAsync();

            log.Info("Blob file was deleted successful");
        }

        public async Task<IEnumerable<string>> GetAllBlobFiles()
        {
            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);
            var files = new List<string>();
            var blobs = containerClient.GetBlobsAsync();

            if (blobs == null)
            {
                log.Error($"GetAllBlobFiles method throws exception {BLOBSTORAGE_IS_EMPTY}");
                throw new ArgumentException(BLOBSTORAGE_IS_EMPTY);
            }

            await foreach (var blob in blobs)
            {
                files.Add(blob.Name);
            }

            return files;
        }

        public async Task DeleteBlobFileAsync(string name)
        {
            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);
            var blobClient = containerClient.GetBlobClient(name);

            if (!blobClient.Exists())
            {
                log.Error($"DeleteBlobFileAsync method throws exception {BLOBFILE_NOT_EXIST}");
                throw new ArgumentException(BLOBFILE_NOT_EXIST);
            }

            await blobClient.DeleteAsync();

            log.Info("Blob file was deleted successful.");
        }

        public IEnumerable<string> GetAllBlobsNames()
        {
            var containerClient = this.blobServiceClient.GetBlobContainerClient(BLOB_STORAGE_CONTAINER);
            var files = new List<string>();
            var blobs = containerClient.GetBlobs();

            foreach (var blob in blobs)
            {
                files.Add(blob.Name);
            }

            return files;
        }
    }
}
