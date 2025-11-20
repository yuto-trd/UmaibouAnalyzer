using System.Text;
using System.Text.Json;
using UmaibouAnalyzer.Api.Models;

namespace UmaibouAnalyzer.Api.Services;

public class RendererService : IRendererService
{
    private readonly HttpClient _httpClient;
    private readonly string _rendererUrl;

    public RendererService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _rendererUrl = configuration["RendererApiUrl"] ?? throw new InvalidOperationException("RendererApiUrl not configured");
    }

    public async Task<List<byte[]>> RenderFromMultipleViewpoints(Stream usdzFileStream, string fileName)
    {
        var viewpoints = GetViewpoints();
        var renderedImages = new List<byte[]>();

        foreach (var viewpoint in viewpoints)
        {
            using var ms = new MemoryStream();
            await usdzFileStream.CopyToAsync(ms);
            usdzFileStream.Position = 0;
            ms.Position = 0;

            var imageBytes = await RenderImage(ms, fileName, viewpoint);
            renderedImages.Add(imageBytes);
        }

        return renderedImages;
    }

    private async Task<byte[]> RenderImage(Stream usdzFileStream, string fileName, ViewpointRequest viewpoint)
    {
        using var content = new MultipartFormDataContent();

        // Add USDZ file
        var fileContent = new StreamContent(usdzFileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("model/vnd.usdz+zip");
        content.Add(fileContent, "usdzFile", fileName);

        // Add viewpoint JSON
        var viewpointJson = JsonSerializer.Serialize(viewpoint);
        var jsonContent = new StringContent(viewpointJson, Encoding.UTF8, "application/json");
        content.Add(jsonContent, "viewpointJson");

        var response = await _httpClient.PostAsync($"{_rendererUrl}/render", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }

    private List<ViewpointRequest> GetViewpoints()
    {
        return new List<ViewpointRequest>
        {
            // 斜め上から俯瞰
            new ViewpointRequest
            {
                PositionX = 2.0,
                PositionY = 2.5,
                PositionZ = 2.0,
                TargetX = 0.0,
                TargetY = 0.0,
                TargetZ = 0.0,
                Fov = 45.0,
                Width = 800,
                Height = 600,
                EnableLighting = false
            },
            // 真横から
            new ViewpointRequest
            {
                PositionX = 3.0,
                PositionY = 0.0,
                PositionZ = 0.0,
                TargetX = 0.0,
                TargetY = 0.0,
                TargetZ = 0.0,
                Fov = 45.0,
                Width = 800,
                Height = 600,
                EnableLighting = false
            },
            // 真上から
            new ViewpointRequest
            {
                PositionX = 0.0,
                PositionY = 3.0,
                PositionZ = 0.0,
                TargetX = 0.0,
                TargetY = 0.0,
                TargetZ = 0.0,
                Fov = 45.0,
                Width = 800,
                Height = 600,
                EnableLighting = false
            }
        };
    }
}
