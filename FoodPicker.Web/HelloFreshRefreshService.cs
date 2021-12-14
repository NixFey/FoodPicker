using System;
using System.Threading;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoodPicker.Web
{
    public class HelloFreshRefreshService : IHostedService
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public HelloFreshRefreshService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CycleRefreshToken, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void CycleRefreshToken(object _)
        {
            using var scope = _scopeFactory.CreateScope();
            var mealService = (HelloFreshMealService)scope.ServiceProvider.GetRequiredService<MealService>();
            await mealService.RefreshAuthentication();
        }
    }
}