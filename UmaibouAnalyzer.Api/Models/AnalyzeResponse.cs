namespace UmaibouAnalyzer.Api.Models;

public class AnalyzeResponse
{
    public bool Success { get; set; }
    public MonsterStats? Data { get; set; }
    public string? ErrorMessage { get; set; }
}
