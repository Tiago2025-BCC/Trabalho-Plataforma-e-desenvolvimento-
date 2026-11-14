using System;
using System.Collections.Generic;

namespace Vendinha.Modelos
{
    public class Cliente
    {
        public int Id {  get; set; }

        public string NomeCompleto { get; set; } = string.Empty;
        
        public string CPF { get; set; } = string.Empty;

        public DateTime DataNascimento { get; set; }
        
        public string? Email { get; set; }

        public ICollection<Divida> Dividas { get; set; } = new List<Divida>();

    }
   
}