using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParanaBanco.Propostas.Infrastructure.Consumers;
using ParanaBanco.Propostas.Infrastructure.Persistence;

namespace ParanaBanco.Propostas.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<PropostasDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("PropostasDb")));


            services.AddMassTransit(x =>
            {
                x.AddConsumer<ClienteCadastradoConsumer>();

                // Outbox persistente com Entity Framework
                x.AddEntityFrameworkOutbox<PropostasDbContext>(options =>
                {
                    options.UseSqlServer();
                    options.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("propostas-cliente-cadastrado", e =>
                    {
                        e.UseMessageRetry(r =>
                            r.Intervals(
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3),
                                TimeSpan.FromSeconds(5)));

                        // Consumer Outbox: controla recebimento,
                        // processamento e publicação de mensagens.
                        e.UseEntityFrameworkOutbox<PropostasDbContext>(context);

                        e.ConfigureConsumer<ClienteCadastradoConsumer>(context);
                    });
                });
            });

            return services;
        }
    }
}
