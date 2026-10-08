
using Commerce.Api.Models;

namespace Commerce.Api.Repositories;

public interface ICommerceRepository
{
    Task<int> ImportAsync(
        string nombreArchivo,
        string hash,
        IReadOnlyList<CommerceRecord> registros,
        CancellationToken ct);

    Task<int> ProcessAsync(DateTime fecha, CancellationToken ct);

    Task<IReadOnlyList<QuarantineRecord>> GetQuarantineAsync(
        CancellationToken ct);

    Task<string> GetDatabaseNameAsync(CancellationToken ct);

    Task<IReadOnlyList<CommerceListItem>> GetCommerceAsync(
    DateTime? fecha,
    CancellationToken ct);
}
