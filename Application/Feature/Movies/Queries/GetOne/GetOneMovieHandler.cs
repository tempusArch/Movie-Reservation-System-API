using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieReservationSystemAPI.Infrastructure;

namespace MovieReservationSystemAPI.Application;

public class GetOneMovieHandler : IRequestHandler<GetOneMovieQuery, ReadMovieDto> {
    private readonly MovieReservationSystemApiDbContext _context;
    private readonly IMapper _mapper;
    public GetOneMovieHandler(MovieReservationSystemApiDbContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReadMovieDto> Handle(GetOneMovieQuery query, CancellationToken cancellationToken) {
        var theOne = await _context.MovieTable
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new ReadMovieDto {
                MovieId = x.Id,
                Title = x.Title,
                Description = x.Description,
                Duration = x.Duration,

                GenreRisuto = x.GenreRisuto
                    .Select(z => new ReadGenreDto {
                        GenreId = z.Id,
                        GenreName = z.Name
                    })
                    .ToList(),

                ShowtimeRisuto = x.ShowtimeRisuto
                    .Select(s => new ReadShowtimeDto {
                        ShowtimeId = s.Id,
                        StartTime = s.StartTime,
                        Duration = s.Duration,
                        EndTime = s.EndTime,
                        Price = s.Price,
                        HallId = s.HallId,
                        HallName = s.Hall.Name
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (theOne == null)
            throw new NotFoundException("Movie not found");

        return theOne;
    }
}
