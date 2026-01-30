using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;
using System.Runtime.CompilerServices;

namespace TestBlobStorage
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string accountName = "";
                string containerName = "";
                BlobServiceClient client;
                do
                {
                    Console.WriteLine("What's your account now");
                    accountName = Console.ReadLine();
                } while (!GetStorageClient(accountName, out client));
                do
                {
                    Console.WriteLine("What's your container name");
                    containerName = Console.ReadLine();
                } while (String.IsNullOrEmpty(containerName));
                Console.WriteLine($"Ceating Blob Container {containerName}");
                BlobContainerClient containerClient = CreateBlobContainer(containerName, client);
                if (containerClient != null)
                {
                    Console.WriteLine("Succeeded");
                }
                Console.WriteLine("Creating a local file for upload to Blob storage...");
                string localPath = "./data/";
                string fileName = "wtfile" + Guid.NewGuid().ToString() + ".txt";
                string localFilePath = Path.Combine(localPath, fileName);

                // Write text to the file
                BlobClient blobClient = await CreateTestFile(containerClient, fileName, localFilePath);

                Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}", blobClient.Uri);

                // Open the file and upload its data
                bool blobExists = await UploadFileToBlobStorage(localFilePath, blobClient);
                if (blobExists)
                {
                    Console.WriteLine("File uploaded successfully, press 'Enter' to continue.");
                    Console.ReadLine();

                    Console.WriteLine("Listing blobs in container...");
                    ListFilesInContainer(containerClient);

                    Console.WriteLine("Press 'Enter' to continue.");
                    Console.ReadLine();
                    string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");

                    Console.WriteLine("Downloading blob to: {0}", downloadFilePath);

                    await DownloadBlobFromStorageContainer(blobClient, downloadFilePath);

                    Console.WriteLine("Blob downloaded successfully to: {0}", downloadFilePath);

                }
                else
                {
                    Console.WriteLine("File upload failed, exiting program..");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
                
        }

        private static BlobContainerClient CreateBlobContainer(string containerName, BlobServiceClient client)
        {
            BlobContainerClient container = client.GetBlobContainerClient(containerName);
            if (container == null)
            {
                return client.CreateBlobContainer(containerName, Azure.Storage.Blobs.Models.PublicAccessType.None).Value;
            }
            return container;
        }

        private static async Task DownloadBlobFromStorageContainer(BlobClient blobClient, string downloadFilePath)
        {
            // Download the blob's contents and save it to a file
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                await download.Content.CopyToAsync(downloadFileStream);
            }
        }

        private static async Task ListFilesInContainer(BlobContainerClient containerClient)
        {
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }
        }

        private static async Task<bool> UploadFileToBlobStorage(string localFilePath, BlobClient blobClient)
        {
            using (FileStream uploadFileStream = File.OpenRead(localFilePath))
            {
                await blobClient.UploadAsync(uploadFileStream);
                uploadFileStream.Close();
            }

            // Verify if the file was uploaded successfully
            bool blobExists = await blobClient.ExistsAsync();
            return blobExists;
        }

        private static async Task<BlobClient> CreateTestFile(BlobContainerClient containerClient, string fileName, string localFilePath)
        {
            await File.WriteAllTextAsync(localFilePath, "Hello, World!");
            Console.WriteLine("Local file created, press 'Enter' to continue.");
            Console.ReadLine();
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            return blobClient;
        }

        static bool GetStorageClient(string accountName, out BlobServiceClient client)
        {
            client = null;
            try
            {
                client = new(
                new Uri($"https://{accountName}.blob.core.windows.net"),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeEnvironmentCredential = true,
                    ExcludeManagedIdentityCredential = true,
                    ExcludeInteractiveBrowserCredential = false
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
    }

    
}
