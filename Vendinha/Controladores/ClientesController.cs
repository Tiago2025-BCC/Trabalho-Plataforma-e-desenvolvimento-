using Microsoft.AspNetCore.Mvc;
using Vendinha.ObjetosTransferencia;
using Vendinha.Servico;
using Vendinha.Servicos;

namespace Vendinha.Controladores
{
    [ApiController]
    [Route("clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly ClienteServico _servico;

        public ClientesController(ClienteServico servico)
        {
            _servico = servico;
        }

        [HttpPost]
        public ActionResult<ClienteRespostaDto> Criar(ClienteCriarDto dto)
        {
            var erro = ValidarDados(dto.NomeCompleto, dto.CPF, dto.DataNascimento);
            if (erro != null)
                return BadRequest(new { erro });

            if (_servico.CpfJaExiste(dto.CPF))
                return Conflict(new { erro = "Já existe um cliente com esse CPF." });

            var criado = _servico.CriarCliente(dto);

            return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
        }

        [HttpGet]
        public ActionResult<List<ClienteRespostaDto>> Listar()
        {
            return Ok(_servico.ListarClientes());
        }

        [HttpGet("{id}")]
        public ActionResult<ClienteRespostaDto> BuscarPorId(int id)
        {
            var cliente = _servico.BuscarCliente(id);

            if (cliente == null)
                return NotFound(new { erro = "Cliente não encontrado." });

            return Ok(cliente);
        }

        [HttpPut("{id}")]
        public ActionResult<ClienteRespostaDto> Atualizar(int id, ClienteAtualizarDto dto)
        {
            var erro = ValidarDados(dto.NomeCompleto, dto.CPF, dto.DataNascimento);
            if (erro != null)
                return BadRequest(new { erro });

            if (_servico.CpfJaExiste(dto.CPF, id))
                return Conflict(new { erro = "Já existe outro cliente com esse CPF." });

            var atualizado = _servico.AtualizarCliente(id, dto);

            if (atualizado == null)
                return NotFound(new { erro = "Cliente não encontrado." });

            return Ok(atualizado);
        }

        [HttpDelete("{id}")]
        public IActionResult Excluir(int id)
        {
            var excluiu = _servico.ExcluirCliente(id);

            if (!excluiu)
                return NotFound(new { erro = "Cliente não encontrado." });

            return NoContent();
        }

        private static string? ValidarDados(string nome, string cpf, DateOnly dataNascimento)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return "O nome completo é obrigatório.";

            if (!ValidadorCpf.EhValido(cpf))
                return "CPF inválido.";

            if (dataNascimento > DateOnly.FromDateTime(DateTime.Today))
                return "A data de nascimento não pode ser no futuro.";

            return null;
        }
    }
}
