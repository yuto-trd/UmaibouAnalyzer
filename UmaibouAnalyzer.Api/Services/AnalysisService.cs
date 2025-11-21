using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using UmaibouAnalyzer.Api.Models;

namespace UmaibouAnalyzer.Api.Services;

public class AnalysisService : IAnalysisService
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _modelName;

    public AnalysisService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not configured");
        var endpoint = configuration["OpenAI:Endpoint"];
        _openAIClient = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions()
            {
                Endpoint = endpoint != null ? new Uri(endpoint) : null
            });
        _modelName = configuration["OpenAI:Model"] ?? "gpt-4o";
    }

    public async Task<MonsterStats> AnalyzeImages(List<byte[]> images)
    {
        var chatClient = _openAIClient.GetChatClient(_modelName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(GetSystemPrompt())
        };

        // Add user message with images
        var userMessageParts = new List<ChatMessageContentPart>();

        for (int i = 0; i < images.Count; i++)
        {
            var imageData = BinaryData.FromBytes(images[i]);
            userMessageParts.Add(ChatMessageContentPart.CreateImagePart(imageData, "image/png"));
        }

        messages.Add(new UserChatMessage(userMessageParts));

        // Request with structured output
        var chatCompletionOptions = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "monster_stats",
                jsonSchema: BinaryData.FromString(@"
                {
                    ""type"": ""object"",
                    ""properties"": {
                        ""name"": {
                            ""type"": ""string"",
                            ""description"": ""画像が表しているものの名前""
                        },
                        ""hp"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""体力（0-100）""
                        },
                        ""speed"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""行動速度（0-100）""
                        },
                        ""short_range_attack_power"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""近接攻撃力（0-100）""
                        },
                        ""long_range_attack_power"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""遠距離攻撃力（0-100）""
                        },
                        ""attack_range"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""攻撃範囲（0-100）""
                        },
                        ""attack_cooldown"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""攻撃クールダウン（0-100）""
                        },
                        ""attack_speed"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""攻撃速度（0-100）""
                        },
                        ""defense_power"": {
                            ""type"": ""integer"",
                            ""minimum"": 0,
                            ""maximum"": 100,
                            ""description"": ""防御力（0-100）""
                        },
                        ""type"": {
                            ""type"": ""string"",
                            ""enum"": [""red"", ""blue"", ""green"", ""yellow"", ""brown"", ""white"", ""black""],
                            ""description"": ""属性タイプ""
                        }
                    },
                    ""required"": [""name"", ""hp"", ""speed"", ""short_range_attack_power"", ""long_range_attack_power"", ""attack_range"", ""attack_cooldown"", ""attack_speed"", ""defense_power"", ""type""],
                    ""additionalProperties"": false
                }
                "),
                jsonSchemaIsStrict: true
            )
        };

        var response = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

        var resultJson = response.Value.Content[0].Text;
        var monsterStats = System.Text.Json.JsonSerializer.Deserialize<MonsterStats>(resultJson);

        return monsterStats ?? throw new InvalidOperationException("Failed to deserialize monster stats");
    }

    private string GetSystemPrompt()
    {
        return """
            ユーザーからスナック菓子をある形に彫刻したものの画像が渡されます．
            1枚目の画像は斜め上から俯瞰したもの，2枚目の画像は真横から見たもの，3枚目の画像は真上から見たものです．
            シルエットが何を表しているか推測して，ゲーム内のモンスターとして扱えるように各種パラメータを算出してください．
            それぞれの数値の項目は0-100の範囲内とします．

            ## 各項目の説明
            **name**
            画像が表しているものの名前です．

            **hp**
            体力．表しているものの元のサイズが大きければHPも多くなります．

            **speed**
            行動速度．hpとは反対にサイズが小さいほど速度が速くなります．

            **short_range_attack_power**
            近接攻撃力．中身が詰まっているもの（例えば金属類）ほど近接攻撃力が高くなります．

            **long_range_attack_power**
            遠距離攻撃力．細長い形状や尖った部分があるほど遠距離攻撃力が高くなります．

            **attack_range**
            攻撃範囲．造形の大きさや広がり具合によって決まります．大きく広がっている造形ほど攻撃範囲が広くなります．

            **attack_cooldown**
            攻撃クールダウン．造形の複雑さや重厚さによって決まります．複雑で重厚な造形ほどクールダウンが長く（値が大きく）なります．

            **attack_speed**
            攻撃速度．中身が詰まっていると攻撃速度が下がります．

            **defense_power**
            防御力．造形が作り込まれているほど防御力が上がります．

            **type**
            特徴．例えばyellowなら雷属性，greenなら風属性，redなら火属性，brownなら土属性です．お菓子の色を元に判別されます．
            """;
    }
}
