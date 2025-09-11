using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using Sophiac.Domain.TestSets;
using Sophiac.Domain.TestRuns;
using Sophiac.Domain.Questions;
using Sophiac.Infrastructure.Repositories;
using System.Threading.Tasks;
using Sophiac.Domain.Answers;

namespace Sophiac.UI.TestSets;

public partial class TestSetExecutionPage : ComponentBase
{
    [Parameter]
    public string TestSetTitle { get; set; }

    [Inject]
    public IQuestionsService Service { get; set; }

    [Inject]
    public ITestRunsRepository runsRepository { get; set; }

    [Inject]
    public NavigationManager Manager { get; set; }

    private Stopwatch _watch = new Stopwatch();

    private TestSet _set = new TestSet();

    private QuestionBase _currentQuestion;

    private TestRun _run = new TestRun();

    protected override async void OnInitialized()
    {
        if (string.IsNullOrEmpty(TestSetTitle))
            return;

        _set = await Service.ReadTestSetAsync(TestSetTitle);
    }

    public void StartRun()
    {
        _currentQuestion = _set.GetNextQuestion();
        _watch.Start();
    }

    public async void PostAnswerActionAsync()
    {
        if (_currentQuestion is SingleChoiceQuestion singleChoiceQuestion)
            _set.SingleChoiceQuestions.Remove(singleChoiceQuestion);

        if (_currentQuestion is MultipleChoicesQuestion multipleChoicesQuestion)
            _set.MultipleChoiceQuestions.Remove(multipleChoicesQuestion);

        if (_currentQuestion is MappingQuestion mappingQuestion)
            _set.MappingQuestions.Remove(mappingQuestion);

        if (_set.Questions.Any() == false)
        {
            _set.IsComplete = true;
            StateHasChanged();
        }
        else
        {
            _currentQuestion = _set.GetNextQuestion();
        }

        StateHasChanged();
    }

    public async Task FinishRun()
    {
    }

    public async Task RecordRun(TestRun run)
    {
        _watch.Reset();
        runsRepository.CreateTestRun(_run);
        var url = $"/testruns/view/{_run.FileName}";
        Manager.NavigateTo(url);
    }
}
