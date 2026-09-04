using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieReservationSystemAPI.Domain;
using MovieReservationSystemAPI.Infrastructure;

namespace MovieReservationSystemAPI.Application;

public class UpdateShowtimeHandler : IRequestHandler<UpdateShowtimeCommand, Showtime> {
    private readonly MovieReservationSystemApiDbContext _context;
    private readonly IMapper _mapper;
    public UpdateShowtimeHandler(MovieReservationSystemApiDbContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Showtime> Handle(UpdateShowtimeCommand command, CancellationToken cancellationToken) {
        var theOne = await _context.ShowtimeTable
            .FirstOrDefaultAsync(st => st.Id == command.ShowtimeId, cancellationToken);

        if (theOne == null)
            throw new NotFoundException("Showtime not found");

        var isExisted = await _context.ShowtimeTable
            .AnyAsync(st => st.StartTime == command.UpdateShowtimeDto.StartTime);

        if (isExisted)
            throw new InvalidOperationException("StartTime already has a Showtime");

        var theHall = await _context.HallTable
            .FirstOrDefaultAsync(x => x.Id == command.UpdateShowtimeDto.HallId, cancellationToken);

        if (theHall == null)
            throw new NotFoundException("Hall not found");

        var theMovie = await _context.MovieTable
            .AsNoTracking()
            .Where(x => x.Id == command.UpdateShowtimeDto.MovieId)
            .Select(x => new {
                x.Duration
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (theMovie == null)
            throw new NotFoundException("Movie not found");

        _mapper.Map(command.UpdateShowtimeDto, theOne);
    
        theOne.Duration = theMovie.Duration;
        theOne.EndTime = theOne.StartTime.Add(theOne.Duration);
  
        var hasOverlap = await _context.ShowtimeTable
            .AnyAsync(x =>
                x.HallId == theOne.HallId &&
                command.UpdateShowtimeDto.StartTime < x.EndTime &&
                theOne.EndTime > x.StartTime);

        if (hasOverlap)
            throw new InvalidOperationException("The Hall has overlapping showtime");
    
        await _context.SaveChangesAsync(cancellationToken);

        return theOne;
    }
}