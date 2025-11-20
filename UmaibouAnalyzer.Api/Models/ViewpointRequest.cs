using System.Text.Json.Serialization;

namespace UmaibouAnalyzer.Api.Models;

public class ViewpointRequest
{
    [JsonPropertyName("positionX")]
    public double PositionX { get; set; }

    [JsonPropertyName("positionY")]
    public double PositionY { get; set; }

    [JsonPropertyName("positionZ")]
    public double PositionZ { get; set; }

    [JsonPropertyName("targetX")]
    public double TargetX { get; set; }

    [JsonPropertyName("targetY")]
    public double TargetY { get; set; }

    [JsonPropertyName("targetZ")]
    public double TargetZ { get; set; }

    [JsonPropertyName("fov")]
    public double Fov { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("enableLighting")]
    public bool EnableLighting { get; set; }
}
