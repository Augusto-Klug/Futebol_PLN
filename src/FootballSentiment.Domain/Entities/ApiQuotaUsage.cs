namespace FootballSentiment.Domain.Entities;

public class ApiQuotaUsage
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Endpoint { get; set; } = null!;
    public int RequestCount { get; set; }
    public int EstimatedUnits { get; set; }
}
