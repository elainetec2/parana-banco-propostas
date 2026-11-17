using System;
using System.Collections.Generic;
using System.Text;

namespace ParanaBanco.Propostas.Application.Events
{    public sealed record PropostaGerada(
        Guid EventId,
        Guid PropostaId,
        Guid ClienteId,
        int Score,
        bool Aprovada,
        decimal LimitePorCartao,
        int QuantidadeMaximaCartoes,
        DateTime OcorridoEm);
}
