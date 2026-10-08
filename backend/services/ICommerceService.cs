
using Commerce.Api.Models;

namespace Commerce.Api.Services;

public interface ICommerceService
{
    Task<int> UploadAsync(IFormFile archivo, CancellationToken ct);

    Task<int> ProcessAsync(DateTime fecha, CancellationToken ct);

    Task<IReadOnlyList<QuarantineRecord>> GetQuarantineAsync(
        CancellationToken ct);

    Task<string> GetDatabaseNameAsync(CancellationToken ct);
    Task<IReadOnlyList<CommerceListItem>> GetCommerceAsync(
    DateTime? fecha,
    CancellationToken ct);
}
