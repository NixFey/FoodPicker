using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using FoodPicker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Infrastructure.Data
{
    public class AutoVoteRepository : EfRepository<AutoVote>
    {
        public AutoVoteRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<AutoVote>> GetAutoVotesForUser(string userId, CancellationToken cancellationToken = default)
        {
            return await DbContext.AutoVotes.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        }
    }
}