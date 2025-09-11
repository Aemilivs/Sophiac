using CommunityToolkit.Maui.Alerts;
using Microsoft.AspNetCore.Components;
using Sophiac.Domain.Questions;
using Sophiac.Domain.TestSets;

namespace Sophiac.UI.TestSets;

public partial class TestSetPage : ComponentBase
{
    [Parameter]
    public string TestSetTitle { get; set; }

    [Inject]
    public IQuestionsService Service { get; set; }

    [Inject]
    public NavigationManager Manager { get; set; }

    private TestSet _set = new TestSet();

    private IList<string> _questionTypes = new List<string>();

    protected override async void OnInitialized()
    {
        if (string.IsNullOrEmpty(TestSetTitle))
            return;

        _set = await Service.ReadTestSetAsync(TestSetTitle);

        if (_set == null)
        {
            Toast.Make("Couldn't load the test set!").Show();
        }
    }

    public void OnQuestionTypeChange(ChangeEventArgs arguments, QuestionBase question)
    {
        var index = _set.Questions.IndexOf(question);

        DeleteQuestion(question);

        var type = arguments.Value.ToString();

        switch (question)
        {
            case SingleChoiceQuestion:
                var singleChoiceQuestion =
                new SingleChoiceQuestion
                {
                    Title = question.Title,
                    Description = question.Description
                };
                _set.SingleChoiceQuestions.Add(singleChoiceQuestion);
                break;
            case MultipleChoicesQuestion:
                var multipleChoicesQuestion =
                new MultipleChoicesQuestion
                {
                    Title = question.Title,
                    Description = question.Description
                };
                _set.MultipleChoiceQuestions.Add(multipleChoicesQuestion);
                break;
            case MappingQuestion:
                var mappingQuestion =
                new MappingQuestion
                {
                    Title = question.Title,
                    Description = question.Description
                };
                _set.MappingQuestions.Add(mappingQuestion);
                break;
        }

        StateHasChanged();
    }

    public void CreateQuestion()
    {
        var question = new SingleChoiceQuestion();
        _set.SingleChoiceQuestions.Insert(0, question);
        _questionTypes = _set.Questions.Select(it => it.GetType().Name).ToList();
    }

    public void DeleteQuestion(QuestionBase question)
    {
        if (question is SingleChoiceQuestion singleChoiceQuestion)
            _set.SingleChoiceQuestions.Remove(singleChoiceQuestion);

        if (question is MultipleChoicesQuestion multipleChoicesQuestion)
            _set.MultipleChoiceQuestions.Remove(multipleChoicesQuestion);

        if (question is MappingQuestion mappingQuestion)
            _set.MappingQuestions.Remove(mappingQuestion);

        _questionTypes = _set.Questions.Select(it => it.GetType().Name).ToList();
        StateHasChanged();
    }

    public async Task SubmitAsync()
    {
        await Service.CreateTestSetAsync(_set);
        Manager.NavigateTo("/testsets");
    }
}
