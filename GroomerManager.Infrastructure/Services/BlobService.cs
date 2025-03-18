using GroomerManager.Application.Common.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SequentialGuid;

namespace GroomerManager.Infrastructure.Services;

public class BlobService(BlobServiceClient blobServiceClient) : IBlobService
{
    private const string ContainerName = "files";
    
    public async Task<(Uri Uri, Guid FileId)> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        
        await containerClient.CreateIfNotExistsAsync();
        
        await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

        var fileId = SequentialGuidGenerator.Instance.NewGuid();

        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return (blobClient.Uri, fileId);
    }
}