using System.Threading;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Infrastructure.Data
{
    public class PersistentConfigRepository : EfRepository<PersistentConfig>
    {
        private readonly ApplicationDbContext _db;

        public PersistentConfigRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;
        }
        
        public async Task<PersistentConfig> GetByCodeOrNull(string configCode, CancellationToken cancellationToken = default)
        {
            return await DbContext.PersistentConfigs.SingleOrDefaultAsync(x => x.ConfigCode == configCode, cancellationToken);
        }

        public async Task<PersistentConfig> UpdateByCode(string configCode, string value, CancellationToken cancellationToken = default)
        {
            var config = await GetByCodeOrNull(configCode, cancellationToken);
            if (config == null) return null;
            config.Value = value;
            await UpdateAsync(config, cancellationToken);

            return config;
        }

    }
}