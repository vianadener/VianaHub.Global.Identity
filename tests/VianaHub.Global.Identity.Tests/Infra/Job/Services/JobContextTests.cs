using VianaHub.Global.Identity.Infra.Job.Services;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Services;

public class JobContextTests
{
    #region EnterBackgroundJobScope

    [Fact(DisplayName = "EnterBackgroundJobScope - Deve definir JobName e ExecutionId no contexto")]
    [Trait("Infra.Job", "")]
    public void EnterBackgroundJobScope_Sucesso_DeveDefinirContexto()
    {
        using (JobContext.EnterBackgroundJobScope("MeuJob", "exec-001"))
        {
            Assert.Equal("MeuJob", JobContext.CurrentJobName);
            Assert.Equal("exec-001", JobContext.CurrentExecutionId);
            Assert.True(JobContext.IsInBackgroundJob);
        }
    }

    [Fact(DisplayName = "EnterBackgroundJobScope - Deve limpar contexto após dispose")]
    [Trait("Infra.Job", "")]
    public void EnterBackgroundJobScope_AposDispose_DeveRestaurarContextoAnterior()
    {
        using (JobContext.EnterBackgroundJobScope("MeuJob", "exec-001"))
        {
            // contexto ativo
        }

        Assert.Null(JobContext.CurrentJobName);
        Assert.Null(JobContext.CurrentExecutionId);
        Assert.False(JobContext.IsInBackgroundJob);
    }

    [Fact(DisplayName = "EnterBackgroundJobScope - Deve suportar escopos aninhados")]
    [Trait("Infra.Job", "")]
    public void EnterBackgroundJobScope_EscoposAninhados_DeveRestaurarContextoExterno()
    {
        using (JobContext.EnterBackgroundJobScope("JobExterno", "exec-ext"))
        {
            Assert.Equal("JobExterno", JobContext.CurrentJobName);

            using (JobContext.EnterBackgroundJobScope("JobInterno", "exec-int"))
            {
                Assert.Equal("JobInterno", JobContext.CurrentJobName);
                Assert.Equal("exec-int", JobContext.CurrentExecutionId);
            }

            Assert.Equal("JobExterno", JobContext.CurrentJobName);
            Assert.Equal("exec-ext", JobContext.CurrentExecutionId);
        }
    }

    [Fact(DisplayName = "EnterBackgroundJobScope - Deve aceitar valores nulos")]
    [Trait("Infra.Job", "")]
    public void EnterBackgroundJobScope_ValoresNulos_DeveDefinirNullNoContexto()
    {
        using (JobContext.EnterBackgroundJobScope(null, null))
        {
            Assert.Null(JobContext.CurrentJobName);
            Assert.Null(JobContext.CurrentExecutionId);
            Assert.True(JobContext.IsInBackgroundJob);
        }
    }

    [Fact(DisplayName = "IsInBackgroundJob - Deve retornar false fora de qualquer escopo")]
    [Trait("Infra.Job", "")]
    public void IsInBackgroundJob_ForaDeEscopo_DeveRetornarFalse()
    {
        Assert.False(JobContext.IsInBackgroundJob);
    }

    #endregion
}
