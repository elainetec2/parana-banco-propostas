using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace ParanaBanco.Propostas.Domain.Services
{
    public static class CalculadoraScore
    {
        public static int Calcular(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException(
                    "CPF é obrigatório.",
                    nameof(cpf));

            var cpfNormalizado = new string(
                cpf.Where(char.IsDigit).ToArray());

            if (cpfNormalizado.Length != 11)
                throw new ArgumentException(
                    "CPF deve conter 11 dígitos.",
                    nameof(cpf));

            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(cpfNormalizado));

            var valor = BitConverter.ToUInt32(hash, 0);

            return (int)(valor % 1001);
        }
    }
}
