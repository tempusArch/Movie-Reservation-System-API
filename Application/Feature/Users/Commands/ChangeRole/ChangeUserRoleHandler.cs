using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieReservationSystemAPI.Domain;
using MovieReservationSystemAPI.Infrastructure;

namespace MovieReservationSystemAPI.Application;

public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, Unit> {
    private readonly MovieReservationSystemApiDbContext _context;
    private readonly IHttpContextService _httpContextService;
    public ChangeUserRoleHandler(MovieReservationSystemApiDbContext context, IHttpContextService httpContextService) {
        _context = context;
        _httpContextService = httpContextService;
    }

    public async Task<Unit> Handle(ChangeUserRoleCommand command, CancellationToken cancellationToken) {
        if (command.CurrentUserId == command.ChangeUserRoleDto.TargetUserId)
            throw new InvalidOperationException("Can not change your own role");

        var theOne = await _context.UserTable
            .FirstOrDefaultAsync(x => x.Id == command.ChangeUserRoleDto.TargetUserId, cancellationToken);

        if (theOne == null)
            throw new NotFoundException("User not found");

        if (command.ChangeUserRoleDto.UserRole == UserRole.Admin)
            theOne.Role = "Admin";

        if (command.ChangeUserRoleDto.UserRole == UserRole.User)
            theOne.Role = "User"; 

        await _context.SaveChangesAsync(cancellationToken);        

        return Unit.Value;
    }
}