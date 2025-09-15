using System.Text.Json.Serialization;

namespace Leaderboard.Api;

public class Customer
{
    [JsonPropertyName("CustomerId")]
    public long CustomerId { get; set; }

    [JsonPropertyName("Score")]
    public decimal Score { get; set; }

    [JsonPropertyName("Rank")]
    public int Rank { get; set; }
}