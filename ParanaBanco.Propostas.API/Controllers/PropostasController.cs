using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParanaBanco.Propostas.Infrastructure.Persistence;

namespace ParanaBanco.Propostas.API.Controllers
{
    [ApiController]
    [Route("api/v1/propostas")]
    public sealed class PropostasController : ControllerBase
    {
        private readonly PropostasDbContext _dbContext;

        public PropostasController(PropostasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{clienteId:guid}")]
        public async Task<IActionResult> ObterPorClienteId(
            Guid clienteId,
            CancellationToken cancellationToken)
        {
            var proposta = await _dbContext.Propostas
                .AsNoTracking()
                .Where(p => p.ClienteId == clienteId)
                .Select(p => new
                {
                    p.Id,
                    p.ClienteId,
                    p.Score,
                    p.Aprovada,
                    p.LimitePorCartao,
                    p.QuantidadeMaximaCartoes,
                    p.CriadaEm
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (proposta is null)
                return NotFound();

            return Ok(proposta);
        }
    }
}
