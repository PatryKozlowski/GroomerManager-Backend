namespace GroomerManager.Application.Common.Interfaces;

public interface IBlobService
{
    Task<(Uri Uri, Guid FileId)> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default);
}