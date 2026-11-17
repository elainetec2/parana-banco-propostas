using System;
using System.Collections.Generic;
using System.Text;

namespace ParanaBanco.Propostas.Domain.Services
{
    public static class RegraDeCredito
    {
        public static ResultadoCredito Avaliar(int score)
        {
            if (score < 0 || score > 1000)
                throw new ArgumentOutOfRangeException(
                    nameof(score),
                    "O score deve estar entre 0 e 1000.");

            if (score <= 100)
                return new ResultadoCredito(
                    score,
                    false,
                    0m,
                    0);

            if (score <= 500)
                return new ResultadoCredito(
                    score,
                    true,
                    1000m,
                    1);

            return new ResultadoCredito(
                score,
                true,
                5000m,
                2);
        }
    }

    public sealed record ResultadoCredito(
        int Score,
        bool Aprovado,
        decimal LimitePorCartao,
        int QuantidadeMaximaCartoes);
}
