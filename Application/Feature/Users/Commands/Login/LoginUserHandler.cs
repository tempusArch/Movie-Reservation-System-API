using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieReservationSystemAPI.Domain;
using MovieReservationSystemAPI.Infrastructure;

namespace MovieReservationSystemAPI.Application;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, string> {
    private readonly MovieReservationSystemApiDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHttpContextService _httpContextService;
    public LoginUserHandler(MovieReservationSystemApiDbContext context, IJwtService jwtService, IPasswordHasher passordHasher, IHttpContextService httpContextService) {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passordHasher;
        _httpContextService = httpContextService;
    }

    public async Task<string> Handle(LoginUserCommand command, CancellationToken cancellationToken) {
        var theUser = await _context.UserTable
            .SingleOrDefaultAsync(x => x.Email == command.LoginUserDto.Email);

        if (theUser == null) 
            throw new UnauthorizedAccessException("User not found");

        var isValidPassword = _passwordHasher.VerifyPassword(
            command.LoginUserDto.Password,
            theUser.PasswordHashed
        );

        if (!isValidPassword) 
            throw new UnauthorizedAccessException("Invalid password");
          
        var accessToken = _jwtService.Generate_JWT(theUser);
        var refreshToken = _jwtService.Generate_RefreshToken(theUser.Id.ToString());

        _context.RefreshTokenTable.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        _httpContextService.SetRefreshToken(refreshToken);

        return accessToken;
    }
}