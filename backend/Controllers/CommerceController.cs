
using Commerce.Api.Models;
using Commerce.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Commerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommerceController : ControllerBase
{
    private readonly ICommerceService _service;
    private readonly ILogger<CommerceController> _logger;

    public CommerceController(
        ICommerceService service,
        ILogger<CommerceController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            mensaje = "API Commerce funcionando",
            estado = "OK"
        });
    }

    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase(
        CancellationToken ct)
    {
        try
        {
            var nombre = await _service.GetDatabaseNameAsync(ct);

            return Ok(new
            {
                mensaje = "Conexión correcta",
                baseDatos = nombre
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error de conexión a SQL Server");

            return StatusCode(500, new
            {
                mensaje = "No se pudo conectar a la base de datos."
            });
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        try
        {
            var cantidad = await _service.UploadAsync(file, ct);

            return Ok(new
            {
                mensaje = "Archivo importado correctamente",
                registrosInsertados = cantidad
            });
        }
        catch (CommerceValidationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
        catch (SqlException ex) when (
            ex.Number == 2601 || ex.Number == 2627)
        {
            return Conflict(new
            {
                mensaje = "Este archivo ya fue importado anteriormente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar comercios");

            return StatusCode(500, new
            {
                mensaje = "No se pudo completar la importación."
            });
        }
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process(
        [FromBody] ProcessCommerceRequest request,
        CancellationToken ct)
    {
        if (request?.ProcessDate == null)
            return BadRequest(new
            {
                mensaje = "Debe indicar una fecha de proceso."
            });

        try
        {
            var cantidad = await _service.ProcessAsync(
                request.ProcessDate.Value, ct);

            return Ok(new
            {
                mensaje = "Procesamiento completado",
                fecha = request.ProcessDate.Value.ToString("yyyy-MM-dd"),
                quarantinedCount = cantidad
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar comercios");

            return StatusCode(500, new
            {
                mensaje = "No se pudieron procesar los registros."
            });
        }
    }

    [HttpGet("quarantine")]
    public async Task<IActionResult> GetQuarantine(
        CancellationToken ct)
    {
        try
        {
            var registros = await _service.GetQuarantineAsync(ct);
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar cuarentena");

            return StatusCode(500, new
            {
                mensaje = "No se pudieron consultar los registros."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCommerce(
        [FromQuery] DateTime? fecha,
        CancellationToken ct)
    {
        try
        {
            var registros = await _service.GetCommerceAsync(fecha, ct);
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercios");

            return StatusCode(500, new
            {
                mensaje = "No se pudieron consultar los comercios."
            });
        }
    }

}
