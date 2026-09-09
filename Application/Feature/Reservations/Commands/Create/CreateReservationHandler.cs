using System.Reflection.Metadata;
using System.Text;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieReservationSystemAPI.Domain;
using MovieReservationSystemAPI.Infrastructure;

namespace MovieReservationSystemAPI.Application;

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, ReadReservationDtoUser> {
    private readonly MovieReservationSystemApiDbContext _context;
    private readonly IMapper _mapper;
    public CreateReservationHandler(MovieReservationSystemApiDbContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReadReservationDtoUser> Handle(CreateReservationCommand command, CancellationToken cancellationToken) {
        var theShowtime = await _context.ShowtimeTable
            .AsNoTracking()
            .Where(st => st.Id == command.CreateReservationDto.ShowtimeId)
            .Select(st => new {
                st.StartTime
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (theShowtime == null)
            throw new NotFoundException("Showtime not found");

        if (DateTime.UtcNow >= theShowtime.StartTime)
            throw new InvalidOperationException("Showtime already started");

        var ifExistingReservedSeat = await _context.ReservedSeatTable
            .AnyAsync(rs =>
                rs.ShowtimeId == command.CreateReservationDto.ShowtimeId &&
                command.CreateReservationDto.SeatIdRisuto.Contains(rs.SeatId),
                cancellationToken
            );

        if (ifExistingReservedSeat)
            throw new InvalidOperationException("Seat unavailable");

        var newReservation = _mapper.Map<Reservation>(command.CreateReservationDto);
        
        var theReservedSeatRisuto = command.CreateReservationDto.SeatIdRisuto   
            .Select(id => new ReservedSeat {
                SeatId = id,
                Reservation = newReservation,
                ShowtimeId = command.CreateReservationDto.ShowtimeId
            })
            .ToList();

        newReservation.UserId = command.UserId;

        _context.ReservationTable.Add(newReservation);
        _context.ReservedSeatTable.AddRange(theReservedSeatRisuto);     

        try {
           
            await _context.SaveChangesAsync(cancellationToken);

        } catch (DbUpdateException ex) {

            if (ex.InnerException?.Message.Contains("IX_ReservedSeat_ShowtimeId_SeatId") == true) {
                throw new InvalidOperationException("Seat unavailable");
            }

            throw;
        }

        var result = await _context.ReservationTable
            .AsNoTracking()
            .Where(r => r.Id == newReservation.Id && r.UserId == command.UserId)
            .Select(r => new ReadReservationDtoUser {
                ReservationId = r.Id,
                
                MovieName = r.Showtime.Movie.Title,
                StartTime = r.Showtime.StartTime,
                Duration = r.Showtime.Movie.Duration,
                TotalPrice = r.ReservedSeatRisuto.Count() * r.Showtime.Price,
                ReservationStatus = r.ReservationStatus,
                HallName = r.Showtime.Hall.Name,
                SeatLabelRisuto = r.ReservedSeatRisuto
                    .Select(rs => $"{rs.Seat.Row}{rs.Seat.LineNumber}")
                    .ToList(),
                CreatedAt = r.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            throw new InvalidOperationException("Reservation creation failed");

        return result;
        

    }
}

