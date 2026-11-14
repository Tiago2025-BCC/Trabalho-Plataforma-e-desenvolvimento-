using Microsoft.AspNetCore.Mvc;
using Vendinha.ObjetosTransferencia;
using Vendinha.Servico;

namespace Vendinha.Controladores
{
    [ApiController]
    public class DividasController : ControllerBase
    {
        private readonly DividaServico _servico;

        public DividasController(DividaServico servico)
        {
            _servico = servico;
        }

        [HttpPost("clientes/{clienteId}/dividas")]
        public ActionResult<DividaRespostaDto> Criar(int clienteId, DividaCriarDto dto)
        {
            if (dto.Valor <= 0)
                return BadRequest(new { erro = "O valor da dívida deve ser maior que zero." });

            if (!_servico.ClienteExiste(clienteId))
                return NotFound(new { erro = "Cliente não encontrado." });

            if (_servico.ClienteTemDividaEmAberto(clienteId))
                return Conflict(new { erro = "Este cliente já possui uma dívida em aberto." });

            var criada = _servico.CriarDivida(clienteId, dto);

            return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
        }

        [HttpGet("clientes/{clienteId}/dividas")]
        public ActionResult<List<DividaRespostaDto>> ListarDoCliente(int clienteId)
        {
            if (!_servico.ClienteExiste(clienteId))
                return NotFound(new { erro = "Cliente não encontrado." });

            return Ok(_servico.ListarDividasDoCliente(clienteId));
        }

        [HttpGet("dividas/{id}")]
        public ActionResult<DividaRespostaDto> BuscarPorId(int id)
        {
            var divida = _servico.BuscarDivida(id);

            if (divida == null)
                return NotFound(new { erro = "Dívida não encontrada." });

            return Ok(divida);
        }

        [HttpPut("dividas/{id}/pagar")]
        public ActionResult<DividaRespostaDto> Pagar(int id)
        {
            var divida = _servico.BuscarDivida(id);

            if (divida == null)
                return NotFound(new { erro = "Dívida não encontrada." });

            if (divida.Paga)
                return Conflict(new { erro = "Esta dívida já foi paga." });

            return Ok(_servico.PagarDivida(id));
        }
    }
}
