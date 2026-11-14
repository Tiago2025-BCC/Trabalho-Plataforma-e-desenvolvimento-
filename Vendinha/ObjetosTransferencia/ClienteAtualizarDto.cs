using System;

namespace Vendinha.ObjetosTransferencia
{
    public class ClienteAtualizarDto
    {
        public string NomeCompleto { get; set; } = string.Empty;

        public string CPF { get; set; } = string.Empty;

        public DateOnly DataNascimento { get; set; }

        public string? Email { get; set; }
    }
   
}