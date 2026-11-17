using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ParanaBanco.Clientes.Application.Events;
using ParanaBanco.Propostas.Application.Events;
using ParanaBanco.Propostas.Domain.Entities;
using ParanaBanco.Propostas.Domain.Services;
using ParanaBanco.Propostas.Infrastructure.Persistence;

namespace ParanaBanco.Propostas.Infrastructure.Consumers
{
    public sealed class ClienteCadastradoConsumer
        : IConsumer<ClienteCadastrado>
    {
        private readonly PropostasDbContext _dbContext;

        public ClienteCadastradoConsumer(
            PropostasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(
            ConsumeContext<ClienteCadastrado> context)
        {
            var evento = context.Message;

            // Evita criar outra proposta para o mesmo cliente.
            var propostaExistente = await _dbContext.Propostas
                .AnyAsync(
                    p => p.ClienteId == evento.ClienteId,
                    context.CancellationToken);

            if (propostaExistente)
                return;

            // Simulação determinística de score.
            var score = CalculadoraScore.Calcular(evento.Cpf);

            // Aplica as regras de crédito.
            var proposta = new Proposta(evento.ClienteId, score);

            // Persiste a proposta.
            _dbContext.Propostas.Add(proposta);

            await _dbContext.SaveChangesAsync(
                context.CancellationToken);

            // Publica o resultado para o microsserviço de Cartões.
            await context.Publish(
                new PropostaGerada(
                    Guid.NewGuid(),
                    proposta.Id,
                    proposta.ClienteId,
                    proposta.Score,
                    proposta.Aprovada,
                    proposta.LimitePorCartao,
                    proposta.QuantidadeMaximaCartoes,
                    DateTime.UtcNow));
        }
    }
}
