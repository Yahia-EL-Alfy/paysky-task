using Microsoft.EntityFrameworkCore;
using paysky_task.Data;

namespace paysky_task.Services
{
    public class VacancyArchiverService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VacancyArchiverService> _logger;
        public VacancyArchiverService(IServiceProvider serviceProvider, ILogger<VacancyArchiverService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var now = DateTime.UtcNow;
                var expiredVacancies = await db.Vacancies
                    .Where(v => v.ExpiryDate <= now && !v.IsArchived)
                    .ToListAsync(stoppingToken);
                foreach (var vacancy in expiredVacancies)
                {
                    vacancy.IsArchived = true;
                    vacancy.IsActive = false;
                    _logger.LogInformation($"Archived vacancy {vacancy.Id} - {vacancy.Title}");
                }
                await db.SaveChangesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Run every hour
            }
        }
    }
}
