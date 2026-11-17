using System;
using System.Collections.Generic;
using System.Text;
using ParanaBanco.Propostas.Domain.Services;
using Xunit;

namespace ParanaBanco.Propostas.UnitTests
{
    public class RegraDeCreditoTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void Avaliar_ScoreAte100_DeveNegarCredito(int score)
        {
            // Act
            var resultado = RegraDeCredito.Avaliar(score);

            // Assert
            Assert.Equal(score, resultado.Score);
            Assert.False(resultado.Aprovado);
            Assert.Equal(0m, resultado.LimitePorCartao);
            Assert.Equal(0, resultado.QuantidadeMaximaCartoes);
        }

        [Theory]
        [InlineData(101)]
        [InlineData(300)]
        [InlineData(500)]
        public void Avaliar_ScoreEntre101E500_DeveAprovarUmCartao(int score)
        {
            // Act
            var resultado = RegraDeCredito.Avaliar(score);

            // Assert
            Assert.Equal(score, resultado.Score);
            Assert.True(resultado.Aprovado);
            Assert.Equal(1000m, resultado.LimitePorCartao);
            Assert.Equal(1, resultado.QuantidadeMaximaCartoes);
        }

        [Theory]
        [InlineData(501)]
        [InlineData(750)]
        [InlineData(1000)]
        public void Avaliar_ScoreEntre501E1000_DeveAprovarDoisCartoes(int score)
        {
            // Act
            var resultado = RegraDeCredito.Avaliar(score);

            // Assert
            Assert.Equal(score, resultado.Score);
            Assert.True(resultado.Aprovado);
            Assert.Equal(5000m, resultado.LimitePorCartao);
            Assert.Equal(2, resultado.QuantidadeMaximaCartoes);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1001)]
        public void Avaliar_ScoreForaDoIntervalo_DeveLancarExcecao(int score)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => RegraDeCredito.Avaliar(score));
        }
    }
}
