
using Commerce.Api.Models;
using Commerce.Api.Repositories;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Commerce.Api.Services;

public class CommerceService : ICommerceService
{
    private readonly ICommerceRepository _repository;

    public CommerceService(ICommerceRepository repository)
    {
        _repository = repository;
    }


    public Task<IReadOnlyList<CommerceListItem>> GetCommerceAsync(
        DateTime? fecha,
        CancellationToken ct)
    {
        return _repository.GetCommerceAsync(fecha, ct);
    }


    public async Task<int> UploadAsync(
        IFormFile archivo, CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
            throw new CommerceValidationException(
                "El archivo está vacío.");

        var coincidencia = Regex.Match(
            archivo.FileName,
            @"^commerce_(\d{8})\.csv$",
            RegexOptions.IgnoreCase);

        if (!coincidencia.Success)
            throw new CommerceValidationException(
                "El nombre debe tener el formato commerce_DDMMYYYY.csv.");

        if (!DateTime.TryParseExact(
            coincidencia.Groups[1].Value,
            "ddMMyyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _))
        {
            throw new CommerceValidationException(
                "La fecha del nombre del archivo no es válida.");
        }

        string hash;

        using (var stream = archivo.OpenReadStream())
        {
            var bytes = await SHA256.HashDataAsync(stream, ct);
            hash = Convert.ToHexString(bytes);
        }

        var registros = new List<CommerceRecord>();

        try
        {
            using var reader = new StreamReader(
                archivo.OpenReadStream());

            using var csv = new CsvReader(
                reader, CultureInfo.InvariantCulture);

            if (!await csv.ReadAsync())
                throw new CommerceValidationException(
                    "El CSV no contiene información.");

            csv.ReadHeader();

            var columnas = new[]
            {
                "pc_processdate",
                "pc_nomcomred",
                "pc_numdoc"
            };

            var encabezados = csv.HeaderRecord ?? [];

            if (!columnas.All(c => encabezados.Contains(c)))
                throw new CommerceValidationException(
                    "El archivo no contiene las columnas requeridas.");

            while (await csv.ReadAsync())
            {
                var fechaTexto =
                    csv.GetField("pc_processdate") ?? "";

                var nombre =
                    csv.GetField("pc_nomcomred") ?? "";

                var documento =
                    csv.GetField("pc_numdoc") ?? "";

                if (!DateTime.TryParseExact(
                    fechaTexto,
                    ["yyyy-MM-dd", "dd/MM/yyyy", "yyyyMMdd"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fecha))
                {
                    throw new CommerceValidationException(
                        $"Fecha de proceso inválida: {fechaTexto}");
                }

                if (nombre.Length > 200 || documento.Length > 50)
                    throw new CommerceValidationException(
                        "Un campo supera la longitud permitida.");

                registros.Add(new CommerceRecord
                {
                    ProcessDate = fecha,
                    Nombre = nombre,
                    Documento = documento
                });
            }
        }
        catch (CsvHelperException ex)
        {
            throw new CommerceValidationException(
                "El archivo CSV tiene una estructura incorrecta.", ex);
        }

        if (registros.Count == 0)
            throw new CommerceValidationException(
                "El archivo no contiene registros.");

        return await _repository.ImportAsync(
            archivo.FileName, hash, registros, ct);
    }

    public Task<int> ProcessAsync(
        DateTime fecha, CancellationToken ct)
    {
        return _repository.ProcessAsync(fecha, ct);
    }

    public Task<IReadOnlyList<QuarantineRecord>>
        GetQuarantineAsync(CancellationToken ct)
    {
        return _repository.GetQuarantineAsync(ct);
    }

    public Task<string> GetDatabaseNameAsync(CancellationToken ct)
    {
        return _repository.GetDatabaseNameAsync(ct);
    }
}

public class CommerceValidationException : Exception
{
    public CommerceValidationException(string mensaje)
        : base(mensaje)
    {
    }

    public CommerceValidationException(
        string mensaje, Exception inner)
        : base(mensaje, inner)
    {
    }


}
