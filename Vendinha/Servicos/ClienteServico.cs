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
                Email = dto.Email
            };

            _contexto.Clientes.Add(cliente);
            _contexto.SaveChanges();

            return MontarResposta(cliente, 0);
        }

        public List<ClienteRespostaDto> ListarClientes()
        {
            var clientes = _contexto.Clientes.ToList();

            var totais = _contexto.Dividas
                .Where(d => !d.Paga)
                .ToList()
                .GroupBy(d => d.ClienteId)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Valor));

            return clientes.Select(cliente =>
            {
                totais.TryGetValue(cliente.Id, out var total);
                return MontarResposta(cliente, total);
            }).ToList();
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
            cliente.Email = dto.Email;

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

        private static string SomenteNumeros(string texto)
        {
            return new string(texto.Where(char.IsDigit).ToArray());
        }
    }
}
