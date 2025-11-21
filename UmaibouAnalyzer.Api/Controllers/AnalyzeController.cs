using Microsoft.AspNetCore.Mvc;
using UmaibouAnalyzer.Api.Models;
using UmaibouAnalyzer.Api.Services;

namespace UmaibouAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly IRendererService _rendererService;
    private readonly IAnalysisService _analysisService;
    private readonly ILogger<AnalyzeController> _logger;

    public AnalyzeController(
        IRendererService rendererService,
        IAnalysisService analysisService,
        ILogger<AnalyzeController> logger)
    {
        _rendererService = rendererService;
        _analysisService = analysisService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AnalyzeResponse>> Analyze(IFormFile usdzFile)
    {
        try
        {
            // Validate file
            if (usdzFile == null || usdzFile.Length == 0)
            {
                return BadRequest(new AnalyzeResponse
                {
                    Success = false,
                    ErrorMessage = "No file uploaded"
                });
            }

            if (!usdzFile.FileName.EndsWith(".usdz", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new AnalyzeResponse
                {
                    Success = false,
                    ErrorMessage = "File must be a USDZ file"
                });
            }

            _logger.LogInformation("Processing USDZ file: {FileName}", usdzFile.FileName);

            // Step 1: Render images from multiple viewpoints
            _logger.LogInformation("Rendering images from multiple viewpoints");
            List<byte[]> images;
            using (var stream = usdzFile.OpenReadStream())
            {
                images = await _rendererService.RenderFromMultipleViewpoints(stream, usdzFile.FileName);
            }

            _logger.LogInformation("Rendered {Count} images", images.Count);

            // Step 2: Analyze images with OpenAI
            _logger.LogInformation("Analyzing images with OpenAI");
            var monsterStats = await _analysisService.AnalyzeImages(images);

            _logger.LogInformation("Analysis complete: {MonsterName}", monsterStats.Name);

            // Step 3: Validate the output
            var validationError = ValidateMonsterStats(monsterStats);
            if (validationError != null)
            {
                return BadRequest(new AnalyzeResponse
                {
                    Success = false,
                    ErrorMessage = validationError
                });
            }

            return Ok(monsterStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing USDZ file");
            return StatusCode(500, new AnalyzeResponse
            {
                Success = false,
                ErrorMessage = $"Internal server error: {ex.Message}"
            });
        }
    }

    private string? ValidateMonsterStats(MonsterStats stats)
    {
        if (string.IsNullOrWhiteSpace(stats.Name))
            return "Name is required";

        if (stats.Hp < 0 || stats.Hp > 100)
            return "HP must be between 0 and 100";

        if (stats.Speed < 0 || stats.Speed > 100)
            return "Speed must be between 0 and 100";

        if (stats.ShortRangeAttackPower < 0 || stats.ShortRangeAttackPower > 100)
            return "Short range attack power must be between 0 and 100";

        if (stats.LongRangeAttackPower < 0 || stats.LongRangeAttackPower > 100)
            return "Long range attack power must be between 0 and 100";

        if (stats.AttackRange < 0 || stats.AttackRange > 100)
            return "Attack range must be between 0 and 100";

        if (stats.AttackCooldown < 0 || stats.AttackCooldown > 100)
            return "Attack cooldown must be between 0 and 100";

        if (stats.AttackSpeed < 0 || stats.AttackSpeed > 100)
            return "Attack speed must be between 0 and 100";

        if (stats.DefensePower < 0 || stats.DefensePower > 100)
            return "Defense power must be between 0 and 100";

        if (string.IsNullOrWhiteSpace(stats.Type))
            return "Type is required";

        return null;
    }
}
