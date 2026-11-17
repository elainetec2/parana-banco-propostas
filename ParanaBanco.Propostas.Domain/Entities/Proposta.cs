using System;
using System.Collections.Generic;
using System.Text;
using ParanaBanco.Propostas.Domain.Services;

namespace ParanaBanco.Propostas.Domain.Entities
{
    public sealed class Proposta
    {
        public Guid Id { get; private set; }
        public Guid ClienteId { get; private set; }
        public int Score { get; private set; }
        public bool Aprovada { get; private set; }
        public decimal LimitePorCartao { get; private set; }
        public int QuantidadeMaximaCartoes { get; private set; }
        public DateTime CriadaEm { get; private set; }

        private Proposta() { }

        public Proposta(Guid clienteId, int score)
        {
            if (clienteId == Guid.Empty)
                throw new ArgumentException(
                    "O identificador do cliente é obrigatório.",
                    nameof(clienteId));

            var resultado = RegraDeCredito.Avaliar(score);

            Id = Guid.NewGuid();
            ClienteId = clienteId;
            Score = resultado.Score;
            Aprovada = resultado.Aprovado;
            LimitePorCartao = resultado.LimitePorCartao;
            QuantidadeMaximaCartoes = resultado.QuantidadeMaximaCartoes;
            CriadaEm = DateTime.UtcNow;
        }
    }
}
