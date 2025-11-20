using System.Text.Json.Serialization;

namespace UmaibouAnalyzer.Api.Models;

public class MonsterStats
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("hp")]
    public int Hp { get; set; }

    [JsonPropertyName("speed")]
    public int Speed { get; set; }

    [JsonPropertyName("attack_power")]
    public int AttackPower { get; set; }

    [JsonPropertyName("attack_speed")]
    public int AttackSpeed { get; set; }

    [JsonPropertyName("defense_power")]
    public int DefensePower { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
