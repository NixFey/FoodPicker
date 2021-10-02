using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Infrastructure.Data
{
    public class WeekUserCommentRepository : EfRepository<WeekUserComment>
    {
        private readonly ApplicationDbContext _db;

        public WeekUserCommentRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;
        }

        public async Task<List<WeekUserComment>> GetCommentsForWeek(int weekId,
            CancellationToken cancellationToken = default)
        {
            return await _db.WeekUserComments.Include(x => x.User).Where(x => x.WeekId == weekId).ToListAsync(cancellationToken);
        }
        
        public async Task<WeekUserComment> GetCommentForWeekAndUser(int weekId, string userId,
            CancellationToken cancellationToken = default)
        {
            return await _db.WeekUserComments.FirstOrDefaultAsync(x => x.WeekId == weekId && x.UserId == userId,
                cancellationToken);
        }
    }
}