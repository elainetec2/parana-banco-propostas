using System;
using System.Collections.Generic;
using System.Text;

namespace ParanaBanco.Propostas.Application.Events
{
    public sealed record ClienteCadastrado(
        Guid EventId,
        Guid ClienteId,
        string Nome,
        string Cpf,
        string Email,
        DateTime OcorridoEm);
}
