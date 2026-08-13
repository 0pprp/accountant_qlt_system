using Application.Common.Models.File;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface IFileManager
{
    public Task<FileDto> SaveFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    public void Delete(string relativePath);
}
