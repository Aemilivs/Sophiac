using Microsoft.EntityFrameworkCore;
using Sophiac.Domain.TestSets;

namespace Sophiac.Infrastructure.Repositories
{
    public class TestSetRepository : ITestSetRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<TestSet> _testSets;

        public TestSetRepository(DbContext context)
        {
            _context = context;
            _testSets = _context.Set<TestSet>();
        }

        public async Task<IEnumerable<TestSet>> GetAllAsync()
        {
            _context.ChangeTracker.Clear();
            return await _testSets.ToListAsync();
        }

        public async Task<TestSet?> GetByTitleAsync(string title)
        {
            _context.ChangeTracker.Clear();
            return await _testSets.FirstOrDefaultAsync(ts => ts.Title == title);
        }

        public async Task<TestSet?> GetByIdAsync(int id)
        {
            return await _testSets.FindAsync(id);
        }

        public async Task AddAsync(TestSet testSet)
        {
            var existingTestSet = _testSets.FirstOrDefault(it => it.Title.Equals(testSet.Title));

            if (existingTestSet != null)
            {
                testSet.Id = existingTestSet.Id;
                await UpdateAsync(testSet);
                return;
            }

            await _testSets.AddAsync(testSet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TestSet testSet)
        {
            var existingTestSet = _testSets.FirstOrDefault(it => it.Title.Equals(testSet.Title));

            if (existingTestSet == null)
            {
                throw new InvalidOperationException("There is no such test set to update!");
            }

            existingTestSet.SingleChoiceQuestions.Concat(testSet.SingleChoiceQuestions);
            existingTestSet.MultipleChoiceQuestions.Concat(testSet.MultipleChoiceQuestions);
            existingTestSet.MappingQuestions.Concat(testSet.MappingQuestions);
            _context.Update(existingTestSet);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var testSet = await GetByIdAsync(id);
            if (testSet != null)
            {
                _testSets.Remove(testSet);
                await _context.SaveChangesAsync();
            }
        }
    }
}
