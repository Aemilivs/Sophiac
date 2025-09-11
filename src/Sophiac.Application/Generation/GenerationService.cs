using Microsoft.Extensions.AI;
using Sophiac.Application.Questions;
using Sophiac.Domain.Chat;
using Sophiac.Domain.Generation;
using Sophiac.Domain.Settings;

namespace Sophiac.Application.Generation;

public class GenerationService : IGenerationService
{
    private const string SYSTEM_PROMPT = "You are an AI agent that generates questions from a given text using the tools available to you. Follow these instructions precisely:\n\n1. **Understandability check:** \n   - Before generating any questions, ensure the input text is fully understandable.\n   - If the text is unclear or ambiguous, respond only with: \"Input text not understandable. No questions generated.\" Do not attempt question generation.\n   - If the text contains meta information (e.g. Authors names) - ignore it\n\n2. **Question types:** \n   - Generate questions of three types: \n     - Single Choice\n     - Multiple Choice\n     - Mapping Question\n   - Use the corresponding tool for each type. Do not attempt to manually create questions.\n\n3. **Existing questions:** \n   - List all questions already present in the given test set before generating new ones.\n\n4. **Generation constraints:** \n   - Each generated question must have clear options/answers.\n   - Maintain consistency with the content of the input text.\n   - Avoid adding information not present in the text.\n\n5. **Output format:** \n   - Clearly indicate each question type.\n\nFollow these rules strictly. Do not deviate under any circumstances.";
    private readonly IChatProvider _chatProvider;
    private readonly ISettingsRepository _repository;
    private readonly QuestionsTools _tools;
    public GenerationService(IChatProvider chatProvider, ISettingsRepository repository, QuestionsTools tools)
    {
        _chatProvider = chatProvider ?? throw new ArgumentNullException(nameof(chatProvider));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tools = tools ?? throw new ArgumentNullException(nameof(tools));
    }

    public async Task GenerateQuestionAsync(string testSetTitle, string textInput, CancellationToken token)
    {
        var client = await _chatProvider.ProvideAsync();
        client =
            new ChatClientBuilder(client)
                .UseFunctionInvocation()
                .Build();
        var sentences = textInput.Split(['.', '!', '?', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var windowSize = await FetchSetting("llm_window_size", defaultValue: 50);
        var windows = sentences
            .Select((sentence, index) => new { Sentence = sentence, Index = index })
            .GroupBy(it => it.Index / windowSize)
            .Select(it => string.Join(". ", it.Select(x => x.Sentence).ToArray()) + ".")
            .ToList();

        List<ChatMessage> conversation = new();
        var customPrompt = await FetchCustomInstructions();
        ChatMessage message = new(ChatRole.System, SYSTEM_PROMPT);
        conversation.Add(message);
        foreach (var window in windows)
        {
            if (token.IsCancellationRequested)
                break;

            var userPrompt = $"Please review the existing questions in the test set titled `{testSetTitle}`. Then, based on the following text chunk, generate additional  questions in the same style:\n```text\n{window}\n```";
            message = new(ChatRole.User, userPrompt);
            conversation.Add(message);
            ChatOptions options = new()
            {
                Tools =
                [
                    AIFunctionFactory.Create(_tools.ListQuestionsInTestSet),
                    AIFunctionFactory.Create(_tools.IntroduceSingleChoiceQuestion),
                    AIFunctionFactory.Create(_tools.IntroduceMultipleChoiceQuestion),
                    AIFunctionFactory.Create(_tools.IntroduceMappingQuestion),
                ]
            };
            var response = await client.GetResponseAsync(conversation, options);
            message = new ChatMessage(ChatRole.Assistant, response.Text);
            conversation.Add(message);
        }
    }

    private async ValueTask<int> FetchSetting(string key, int defaultValue)
    {
        var setting = await _repository.GetByKeyAsync(key);

        if (setting == null)
            return defaultValue;

        var raw = setting.Value;
        var isInt = int.TryParse(raw, out var value);

        if (isInt == false)
            return defaultValue;

        return value;
    }

    private async Task<string> FetchCustomInstructions()
    {
        var setting = await _repository.GetByKeyAsync("llm_custom_instructions");

        if (setting == null)
            return SYSTEM_PROMPT;

        var customInstructions = setting.Value;
        if (string.IsNullOrEmpty(customInstructions))
            return SYSTEM_PROMPT;

        return string.Concat(SYSTEM_PROMPT, "\n", customInstructions);
    }
}