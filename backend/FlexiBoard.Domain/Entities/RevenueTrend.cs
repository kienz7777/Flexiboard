namespace FlexiBoard.Domain.Entities;

public class RevenueTrend
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
