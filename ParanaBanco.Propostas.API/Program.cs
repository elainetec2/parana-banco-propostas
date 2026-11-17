
using ParanaBanco.Propostas.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Controllers da API
builder.Services.AddControllers();

// Documentação OpenAPI
builder.Services.AddOpenApi();

// SQL Server, RabbitMQ e MassTransit
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
