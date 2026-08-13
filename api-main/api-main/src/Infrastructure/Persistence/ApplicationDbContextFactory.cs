using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContextFactory : IApplicationDbContextFactory
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public ApplicationDbContextFactory(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public IApplicationDbContext CreateDbContext() => _dbContextFactory.CreateDbContext();

    public async Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        await _dbContextFactory.CreateDbContextAsync(cancellationToken);
}
