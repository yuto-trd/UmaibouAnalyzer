namespace UmaibouAnalyzer.Api.Services;

public interface IRendererService
{
    Task<List<byte[]>> RenderFromMultipleViewpoints(Stream usdzFileStream, string fileName);
}
