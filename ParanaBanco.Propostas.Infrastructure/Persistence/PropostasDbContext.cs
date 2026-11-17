using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ParanaBanco.Propostas.Domain.Entities;
using MassTransit;

namespace ParanaBanco.Propostas.Infrastructure.Persistence
{    public sealed class PropostasDbContext : DbContext
    {
        public PropostasDbContext(
            DbContextOptions<PropostasDbContext> options)
            : base(options)
        {
        }

        public DbSet<Proposta> Propostas => Set<Proposta>();

        public DbSet<MassTransit.EntityFrameworkCoreIntegration.InboxState> InboxState
            => Set<MassTransit.EntityFrameworkCoreIntegration.InboxState>();

        public DbSet<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage> OutboxMessage
            => Set<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>();

        public DbSet<MassTransit.EntityFrameworkCoreIntegration.OutboxState> OutboxState
            => Set<MassTransit.EntityFrameworkCoreIntegration.OutboxState>();


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração das tabelas do MassTransit Outbox
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            // Configuração da tabela de propostas
            modelBuilder.Entity<Proposta>(entity =>
            {
                entity.ToTable("Propostas");

                entity.HasKey(p => p.Id);

                entity.HasIndex(p => p.ClienteId)
                    .IsUnique();

                entity.Property(p => p.Score)
                    .IsRequired();

                entity.Property(p => p.LimitePorCartao)
                    .HasPrecision(18, 2);

                entity.Property(p => p.CriadaEm)
                    .IsRequired();
            });
        }
    }
}
