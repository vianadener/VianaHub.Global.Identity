using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Job.Jobs.Cleanup;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Jobs.Cleanup;

public class CleanupExpiredJwtKeysJobTests
{
    private readonly Mock<IJwtKeyDomainService> _jwtKeyServiceMock = new();
    private readonly Mock<ILogger<CleanupExpiredJwtKeysJob>> _loggerMock = new();

    private CleanupExpiredJwtKeysJob CreateSut() =>
        new(_jwtKeyServiceMock.Object, _loggerMock.Object);

    #region Execute

    [Fact(DisplayName = "Execute - Deve limpar chaves expiradas com sucesso quando há chaves para remover")]
    [Trait("Infra.Job", "")]
    public async Task Execute_ComChavesExpiradas_DeveChamarCleanupERegistrarLog()
    {
        _jwtKeyServiceMock
            .Setup(x => x.CleanupExpiredKeysAsync(90, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyServiceMock.Verify(x => x.CleanupExpiredKeysAsync(90, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Execute - Deve completar sem erros quando não há chaves expiradas")]
    [Trait("Infra.Job", "")]
    public async Task Execute_SemChavesExpiradas_DeveCompletarSemErros()
    {
        _jwtKeyServiceMock
            .Setup(x => x.CleanupExpiredKeysAsync(90, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.Execute(CancellationToken.None));

        Assert.Null(exception);
        _jwtKeyServiceMock.Verify(x => x.CleanupExpiredKeysAsync(90, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Execute - Deve propagar exceção quando o serviço de domínio falha")]
    [Trait("Infra.Job", "")]
    public async Task Execute_ExcecaoNoServico_DevePropagarExcecao()
    {
        _jwtKeyServiceMock
            .Setup(x => x.CleanupExpiredKeysAsync(90, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Erro ao limpar chaves"));

        var sut = CreateSut();
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Execute(CancellationToken.None));
    }

    #endregion
}
