
using Commerce.Api.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Commerce.Api.Repositories;

public class CommerceRepository : ICommerceRepository
{
    private readonly IConfiguration _configuration;

    public CommerceRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection CreateConnection()
    {
        var cadena = _configuration.GetConnectionString("CommerceDb")
            ?? throw new InvalidOperationException(
                "No se configuró la conexión CommerceDb.");

        return new SqlConnection(cadena);
    }

    public async Task<int> ImportAsync(
        string nombreArchivo,
        string hash,
        IReadOnlyList<CommerceRecord> registros,
        CancellationToken ct)
    {
        using var conexion = CreateConnection();
        await conexion.OpenAsync(ct);

        using var transaccion = conexion.BeginTransaction();

        try
        {
            using (var comando = new SqlCommand(
                @"INSERT INTO dbo.commerce_imports
                  (file_name, file_hash, record_count)
                  VALUES (@nombre, @hash, @cantidad)",
                conexion, transaccion))
            {
                comando.Parameters.Add(
                    "@nombre", SqlDbType.NVarChar, 260
                ).Value = nombreArchivo;

                comando.Parameters.Add(
                    "@hash", SqlDbType.Char, 64
                ).Value = hash;

                comando.Parameters.Add(
                    "@cantidad", SqlDbType.Int
                ).Value = registros.Count;

                await comando.ExecuteNonQueryAsync(ct);
            }

            var tabla = new DataTable();

            tabla.Columns.Add("pc_processdate", typeof(DateTime));
            tabla.Columns.Add("pc_nomcomred", typeof(string));
            tabla.Columns.Add("pc_numdoc", typeof(string));

            foreach (var registro in registros)
            {
                tabla.Rows.Add(
                    registro.ProcessDate,
                    registro.Nombre,
                    registro.Documento
                );
            }

            using (var comando = new SqlCommand(
                "dbo.sp_create_commerce",
                conexion, transaccion))
            {
                comando.CommandType = CommandType.StoredProcedure;

                var parametro = comando.Parameters.Add(
                    "@registros", SqlDbType.Structured);

                parametro.TypeName = "dbo.CommerceTableType";
                parametro.Value = tabla;

                await comando.ExecuteNonQueryAsync(ct);
            }

            transaccion.Commit();

            return registros.Count;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public async Task<int> ProcessAsync(
        DateTime fecha, CancellationToken ct)
    {
        using var conexion = CreateConnection();
        await conexion.OpenAsync(ct);

        using var comando = new SqlCommand(
            "dbo.sp_process_commerce", conexion);

        comando.CommandType = CommandType.StoredProcedure;

        comando.Parameters.Add(
            "@fecha", SqlDbType.Date
        ).Value = fecha.Date;

        var resultado = await comando.ExecuteScalarAsync(ct);

        return Convert.ToInt32(resultado);
    }

    public async Task<IReadOnlyList<QuarantineRecord>>
        GetQuarantineAsync(CancellationToken ct)
    {
        var registros = new List<QuarantineRecord>();

        using var conexion = CreateConnection();
        await conexion.OpenAsync(ct);

        using var comando = new SqlCommand(
            @"SELECT id, pc_processdate, pc_nomcomred,
                     pc_numdoc, motivo
              FROM dbo.commerce_quarantine
              ORDER BY id DESC", conexion);

        using var lector = await comando.ExecuteReaderAsync(ct);

        while (await lector.ReadAsync(ct))
        {
            registros.Add(new QuarantineRecord
            {
                Id = lector.GetInt32(0),

                ProcessDate = lector.IsDBNull(1)
                    ? null
                    : lector.GetDateTime(1).ToString(
                        "yyyy-MM-dd", CultureInfo.InvariantCulture),

                Nombre = lector.IsDBNull(2)
                    ? null : lector.GetString(2),

                Documento = lector.IsDBNull(3)
                    ? null : lector.GetString(3),

                Motivo = lector.IsDBNull(4)
                    ? null : lector.GetString(4)
            });
        }

        return registros;
    }

    public async Task<string> GetDatabaseNameAsync(
        CancellationToken ct)
    {
        using var conexion = CreateConnection();
        await conexion.OpenAsync(ct);

        using var comando = new SqlCommand(
            "SELECT DB_NAME()", conexion);

        var resultado = await comando.ExecuteScalarAsync(ct);

        return Convert.ToString(resultado) ?? "";
    }

    public async Task<IReadOnlyList<CommerceListItem>> GetCommerceAsync(
        DateTime? fecha,
        CancellationToken ct)
    {
        var registros = new List<CommerceListItem>();

        using var conexion = CreateConnection();
        await conexion.OpenAsync(ct);

        using var comando = new SqlCommand(@"
        SELECT
            id,
            pc_processdate,
            pc_nomcomred,
            pc_numdoc
        FROM dbo.commerce
        WHERE (@fecha IS NULL OR pc_processdate = @fecha)
        ORDER BY id DESC", conexion);

        comando.Parameters.Add(
            "@fecha", SqlDbType.Date
        ).Value = fecha.HasValue
            ? fecha.Value.Date
            : DBNull.Value;

        using var lector = await comando.ExecuteReaderAsync(ct);

        while (await lector.ReadAsync(ct))
        {
            registros.Add(new CommerceListItem
            {
                Id = lector.GetInt32(0),

                ProcessDate = lector.IsDBNull(1)
                    ? null
                    : lector.GetDateTime(1).ToString("yyyy-MM-dd"),

                Nombre = lector.IsDBNull(2)
                    ? null
                    : lector.GetString(2),

                Documento = lector.IsDBNull(3)
                    ? null
                    : lector.GetString(3)
            });
        }

        return registros;
    }

}
