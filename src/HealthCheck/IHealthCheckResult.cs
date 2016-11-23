namespace HealthChecks
{
    public interface IHealthCheckResult
    {
        CheckStatus CheckStatus { get; }
        string Name {get;}
    }
}