using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompuestoController : ControllerBase
    {
        private readonly ICompuestoRepository _compuestoRepository;

        public CompuestoController(ICompuestoRepository compuestoRepository)
        {
            _compuestoRepository = compuestoRepository;
        }

        // GET: api/Compuesto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compuesto>>> GetAllCompuestos()
        {
            var compuestos = await _compuestoRepository.GetAllAsync();
            return Ok(compuestos);
        }

        // GET: api/Compuesto/{uuid}
        [HttpGet("{uuid}")]
        public async Task<ActionResult<Compuesto>> GetCompuestoByUuid(Guid uuid)
        {
            var compuesto = await _compuestoRepository.GetByGuidAsync(uuid);

            if (compuesto == null || compuesto.Uuid == Guid.Empty)
                return NotFound($"Compuesto con UUID {uuid} no encontrado.");

            return Ok(compuesto);
        }

        // POST: api/Compuesto
        [HttpPost]
        public async Task<ActionResult> CreateCompuesto([FromBody] Compuesto compuesto)
        {
            if (compuesto == null)
                return BadRequest("Datos de compuesto inválidos.");

            var creado = await _compuestoRepository.CreateAsync(compuesto);

            if (!creado)
                return StatusCode(500, "Error al crear el compuesto.");

            return CreatedAtAction(nameof(GetCompuestoByUuid), new { uuid = compuesto.Uuid }, compuesto);
        }

        // PUT: api/Compuesto/{uuid}
        [HttpPut("{uuid}")]
        public async Task<ActionResult> UpdateCompuesto(Guid uuid, [FromBody] Compuesto compuesto)
        {
            if (compuesto == null || compuesto.Uuid != uuid)
                return BadRequest("Datos de actualización inválidos.");

            var actualizado = await _compuestoRepository.UpdateAsync(compuesto);

            if (!actualizado)
                return NotFound($"Compuesto con UUID {uuid} no encontrado.");

            return NoContent();
        }

        // DELETE: api/Compuesto/{uuid}
        [HttpDelete("{uuid}")]
        public async Task<ActionResult> DeleteCompuesto(Guid uuid)
        {
            var eliminado = await _compuestoRepository.RemoveAsync(uuid);

            if (!eliminado)
                return NotFound($"Compuesto con UUID {uuid} no encontrado.");

            return NoContent();
        }
    }
}

