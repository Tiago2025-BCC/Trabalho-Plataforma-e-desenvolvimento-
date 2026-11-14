using Microsoft.EntityFrameworkCore;
using Vendinha.Dados;
using Vendinha.Modelos;
using Vendinha.ObjetosTransferencia;

namespace Vendinha.Servico
{
    public class ClienteServico
    {
        private readonly AppDbContext _contexto;

        public ClienteServico(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public ClienteRespostaDto CriarCliente(ClienteCriarDto dto)
        {
            var cliente = new Cliente
            {
                NomeCompleto = dto.NomeCompleto.Trim(),
                CPF = SomenteNumeros(dto.CPF),
                DataNascimento = dto.DataNascimento.ToDateTime(TimeOnly.MinValue),
                Email = LimparEmail(dto.Email)
            };

            _contexto.Clientes.Add(cliente);
            _contexto.SaveChanges();

            return MontarResposta(cliente, 0);
        }

        public PaginaRespostaDto<ClienteRespostaDto> ListarClientes(string? busca, int pagina, int tamanhoPagina = 10)
        {
            var consulta = _contexto.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var padrao = $"%{EscaparParaLike(busca.Trim())}%";
                consulta = consulta.Where(c => EF.Functions.Like(c.NomeCompleto, padrao, "\\"));
            }

            var clientes = consulta.ToList();

            var totais = _contexto.Dividas
                .Where(d => !d.Paga)
                .ToList()
                .GroupBy(d => d.ClienteId)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Valor));

            var ordenados = clientes
                .Select(cliente =>
                {
                    totais.TryGetValue(cliente.Id, out var total);
                    return MontarResposta(cliente, total);
                })
                .OrderByDescending(c => c.TotalDividas)
                .ThenBy(c => c.NomeCompleto, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var itensDaPagina = ordenados
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToList();

            return new PaginaRespostaDto<ClienteRespostaDto>
            {
                Itens = itensDaPagina,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = ordenados.Count,
                TotalPaginas = (int)Math.Ceiling(ordenados.Count / (double)tamanhoPagina)
            };
        }

        public ClienteRespostaDto? BuscarCliente(int id)
        {
            var cliente = _contexto.Clientes.Find(id);

            if (cliente == null)
            {
                return null;
            }

            return MontarResposta(cliente, TotalEmAberto(cliente.Id));
        }

        public ClienteRespostaDto? AtualizarCliente(int id, ClienteAtualizarDto dto)
        {
            var cliente = _contexto.Clientes.Find(id);

            if (cliente == null)
            {
                return null;
            }

            cliente.NomeCompleto = dto.NomeCompleto.Trim();
            cliente.CPF = SomenteNumeros(dto.CPF);
            cliente.DataNascimento = dto.DataNascimento.ToDateTime(TimeOnly.MinValue);
            cliente.Email = LimparEmail(dto.Email);

            _contexto.SaveChanges();

            return MontarResposta(cliente, TotalEmAberto(cliente.Id));
        }

        public bool ExcluirCliente(int id)
        {
            var cliente = _contexto.Clientes.Find(id);

            if (cliente == null)
            {
                return false;
            }

            _contexto.Clientes.Remove(cliente);
            _contexto.SaveChanges();

            return true;
        }

        public bool CpfJaExiste(string cpf, int? ignorarId = null)
        {
            var cpfLimpo = SomenteNumeros(cpf);

            return _contexto.Clientes.Any(c => c.CPF == cpfLimpo && c.Id != ignorarId);
        }

        private decimal TotalEmAberto(int clienteId)
        {
            return _contexto.Dividas
                .Where(d => d.ClienteId == clienteId && !d.Paga)
                .Select(d => d.Valor)
                .ToList()
                .Sum();
        }

        private static ClienteRespostaDto MontarResposta(Cliente cliente, decimal totalDividas)
        {
            return new ClienteRespostaDto
            {
                Id = cliente.Id,
                NomeCompleto = cliente.NomeCompleto,
                CPF = cliente.CPF,
                DataNascimento = DateOnly.FromDateTime(cliente.DataNascimento),
                Idade = CalcularIdade(cliente.DataNascimento),
                Email = cliente.Email,
                TotalDividas = totalDividas
            };
        }

        private static int CalcularIdade(DateTime dataNascimento)
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - dataNascimento.Year;

            if (dataNascimento.Date > hoje.AddYears(-idade))
            {
                idade--;
            }

            return idade;
        }

        private static string EscaparParaLike(string texto)
        {
            return texto
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        private static string? LimparEmail(string? email)
        {
            return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        }

        private static string SomenteNumeros(string texto)
        {
            return new string(texto.Where(char.IsDigit).ToArray());
        }
    }
}
