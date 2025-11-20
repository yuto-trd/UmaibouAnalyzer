using UmaibouAnalyzer.Api.Models;

namespace UmaibouAnalyzer.Api.Services;

public interface IAnalysisService
{
    Task<MonsterStats> AnalyzeImages(List<byte[]> images);
}
