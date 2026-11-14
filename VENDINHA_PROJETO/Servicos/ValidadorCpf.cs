using System;
using System.Linq;

namespace Vendinha.Servicos
{
    public class ValidadorCpf
    {
         public static bool EhValido(string? cpf)
         {
             if (string.IsNullOrWhiteSpace(cpf))
                return false;

             cpf = cpf.Replace(".", "").Replace("-", "").Trim();

             if(cpf.Length != 11)
               return false;

             if (!cpf.All(c => c >= '0' && c <= '9'))
               return false;

             if (cpf.Distinct().Count() == 1)
               return false;

              int soma = 0;

             for (int i = 0; i < 9; i++)
             {
                    soma += (cpf[i] - '0') * (10 - i);                      
             }

             int primeiroDigito = (soma * 10) % 11;

             if (primeiroDigito == 10)
               primeiroDigito = 0;

             if (primeiroDigito != cpf[9] - '0')
               return false;

             soma = 0;

             for (int i = 0; i < 10; i++)
             {
                    soma += (cpf[i] - '0') * (11 - i);
             }

             int segundoDigito = (soma * 10) % 11;

             if (segundoDigito == 10)
                 segundoDigito = 0;

             return segundoDigito == cpf[10] - '0';

         }
    }
}
