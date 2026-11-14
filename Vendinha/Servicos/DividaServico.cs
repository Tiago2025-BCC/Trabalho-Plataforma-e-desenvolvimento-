using Vendinha.Dados;
using Vendinha.Modelos;
using Vendinha.ObjetosTransferencia;

namespace Vendinha.Servico
{
    public class DividaServico
    {
        private readonly AppDbContext _contexto;

        public DividaServico(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool ClienteExiste(int clienteId)
        {
            return _contexto.Clientes.Any(c => c.Id == clienteId);
        }

        public bool ClienteTemDividaEmAberto(int clienteId)
        {
            return _contexto.Dividas.Any(d => d.ClienteId == clienteId && !d.Paga);
        }

        public DividaRespostaDto CriarDivida(int clienteId, DividaCriarDto dto)
        {
            var divida = new Divida
            {
                ClienteId = clienteId,
                Valor = Math.Round(dto.Valor, 2),
                Paga = false,
                DataCriacao = DateTime.Now
            };

            _contexto.Dividas.Add(divida);
            _contexto.SaveChanges();

            return MontarResposta(divida);
        }

        public List<DividaRespostaDto> ListarDividasDoCliente(int clienteId)
        {
            return _contexto.Dividas
                .Where(d => d.ClienteId == clienteId)
                .OrderByDescending(d => d.Id)
                .ToList()
                .Select(MontarResposta)
                .ToList();
        }

        public DividaRespostaDto? BuscarDivida(int id)
        {
            var divida = _contexto.Dividas.Find(id);

            if (divida == null)
            {
                return null;
            }

            return MontarResposta(divida);
        }

        public DividaRespostaDto? PagarDivida(int id)
        {
            var divida = _contexto.Dividas.Find(id);

            if (divida == null)
            {
                return null;
            }

            divida.Paga = true;
            divida.DataPagamento = DateTime.Now;

            _contexto.SaveChanges();

            return MontarResposta(divida);
        }

        private static DividaRespostaDto MontarResposta(Divida divida)
        {
            return new DividaRespostaDto
            {
                Id = divida.Id,
                Valor = divida.Valor,
                Paga = divida.Paga,
                DataCriacao = divida.DataCriacao,
                DataPagamento = divida.DataPagamento,
                ClienteId = divida.ClienteId
            };
        }
    }
}
