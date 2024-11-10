using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompuestoController(CompuestoService compuestoService) : Controller
    {
        private readonly CompuestoService _compuestoService = compuestoService;


        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var losCompuestos = await _compuestoService
                .GetAllAsync();

            return Ok(losCompuestos);
        }

        [HttpGet("{compuesto_guid:Guid}")]
        public async Task<IActionResult> GetByGuidAsync(Guid compuesto_guid)
        {
            try
            {
                var unCompuesto = await _compuestoService
                    .GetByGuidAsync(compuesto_guid);

                return Ok(unCompuesto);
            }
            catch (AppValidationException error)
            {
                return NotFound(error.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(Compuesto unCompuesto)
        {
            try
            {
                var compuestoCreado = await _compuestoService
                    .CreateAsync(unCompuesto);

                return Ok(compuestoCreado);
            }
            catch (AppValidationException error)
            {
                return BadRequest($"Error en la validación: {error.Message}");
            }
            catch (DbOperationException error)
            {
                return BadRequest($"Error en la operación de la DB: {error.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(Compuesto unCompuesto)
        {
            try
            {
                var compuestoActualizado = await _compuestoService
                    .UpdateAsync(unCompuesto);

                return Ok(compuestoActualizado);
            }
            catch (AppValidationException error)
            {
                return BadRequest($"Error de validación: {error.Message}");
            }
            catch (DbOperationException error)
            {
                return BadRequest($"Error de operación en DB: {error.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveAsync(Guid pais_guid)
        {
            try
            {
                var paisEliminado = await _compuestoService
                    .RemoveAsync(pais_guid);

                return Ok(paisEliminado);
            }
            catch (AppValidationException error)
            {
                return BadRequest($"Error de validación: {error.Message}");
            }
            catch (DbOperationException error)
            {
                return BadRequest($"Error de operacion en DB: {error.Message}");
            }
        }
    }
}
