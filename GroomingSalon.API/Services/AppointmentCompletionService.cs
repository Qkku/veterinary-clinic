
using GroomingSalon.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Services;

public class AppointmentCompletionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentCompletionService> _logger;

    public AppointmentCompletionService(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentCompletionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                DateTime completionTime = DateTime.Now.AddHours(-1);

                var appointments = await context.Appointments
                    .Where(a =>
                        (a.Status == "booked" ||
                         a.Status == "confirmed") &&
                        a.EndDatetime <= completionTime)
                    .ToListAsync(stoppingToken);

                foreach (var appointment in appointments)
                {
                    appointment.Status = "completed";
                }

                if (appointments.Count > 0)
                {
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Автоматически завершено записей: {Count}",
                        appointments.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при автоматическом завершении записей.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(30),
                stoppingToken);
        }
    }
}
