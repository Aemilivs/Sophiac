using Sophiac.Domain.Questions;
using Sophiac.Domain.TestSets;

namespace Sophiac.Application.Questions;

public class QuestionsService : IQuestionsService
{
    private ITestSetRepository _repository;
    public QuestionsService(ITestSetRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task CreateTestSetAsync(string testSetTitle)
    {
        var set = new TestSet
        {
            Title = testSetTitle,
            Strategy = QuestionDeliveryStrategies.SequentialDelivery
        };

        await _repository.AddAsync(set);
    }

    public async Task CreateTestSetAsync(TestSet set)
    {
        if (set == null)
        {
            throw new ArgumentNullException(nameof(set));
        }

        await _repository.AddAsync(set);
    }

    public async Task<TestSet?> ReadTestSetAsync(string testSetTitle)
    {
        return await _repository.GetByTitleAsync(testSetTitle);
    }

    public async Task IntroduceQuestion(string testSetTitle, QuestionBase question)
    {
        var set = await _repository.GetByTitleAsync(testSetTitle);

        if (set == null)
        {
            set = new TestSet
            {
                Title = testSetTitle,
                Strategy = QuestionDeliveryStrategies.SequentialDelivery,
                Questions = { question }
            };

            await _repository.AddAsync(set);
            return;
        }

        var questionExist = set.Questions.Any(it => string.Equals(it.Title, question.Title, StringComparison.OrdinalIgnoreCase));

        if (questionExist)
            return;

        if (question is SingleChoiceQuestion singleChoiceQuestion)
                set.SingleChoiceQuestions.Add(singleChoiceQuestion);

        if (question is MultipleChoicesQuestion multipleChoicesQuestion)
            set.MultipleChoiceQuestions.Add(multipleChoicesQuestion);

        if (question is MappingQuestion mappingQuestion)
            set.MappingQuestions.Add(mappingQuestion);
            
        await _repository.UpdateAsync(set);
    }

    public async Task DeleteTestSet(string testSetTitle)
    {
        var set = await _repository.GetByTitleAsync(testSetTitle);

        if (set == null)
        {
            return;
        }

        await _repository.DeleteAsync(set.Id);
    }

    public async Task<IEnumerable<TestSet>> ReadTestSetsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task UpdateTestSetAsync(TestSet testSet)
    {
        await _repository.UpdateAsync(testSet);
    }
}
