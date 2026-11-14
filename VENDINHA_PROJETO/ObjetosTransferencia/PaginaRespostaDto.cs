using System.Collections.Generic;

namespace Vendinha.ObjetosTransferencia
{
    public class PaginaRespostaDto<T>
    {
        public List<T> Itens { get; set; } = new List<T>();

        public int Pagina { get; set; }

        public int TamanhoPagina { get; set; }

        public int TotalItens { get; set; }

        public int TotalPaginas { get; set; }
    }
}
