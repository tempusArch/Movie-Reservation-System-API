using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieReservationSystemAPI.Domain;
using MovieReservationSystemAPI.Application;

namespace MovieReservationSystemAPI.Infrastructure;

public class ReservationStatusCompleteBackgroudJob : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactor;

    public ReservationStatusCompleteBackgroudJob(IServiceScopeFactory scopeFactory) {
        _scopeFactor = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            using (var scope = _scopeFactor.CreateScope()) {
                
                var _context = scope.ServiceProvider
                    .GetRequiredService<MovieReservationSystemApiDbContext>();

                await _context.ReservationTable
                    .Where(r =>
                        r.ReservationStatus == ReservationStatus.Processing &&
                        r.Showtime.EndTime <= DateTime.UtcNow)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            r => r.ReservationStatus,
                            ReservationStatus.Completed
                        ),
                        cancellationToken
                    );
            
            }

            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        }
    }
}