# Movie_Reservation_System_API

## Tech Stack
- C#
- .NET 8
- ASP.NET Core
- EF Core
- SQL Server
- AutoMapper
- MediatR
- FluentValidation

## Architecture
Clean Architecture + CQRS

## Features
- JWT User Authentication and Role-based Authorization
- Admin management for Genre, Movie, Hall, Seat, Showtime
- Seat reservation and Showtime scheduling
- Concurrency Handling (Database Constraint, unique composite index to prevent duplicate seat reservations)
- MediatR Pipeline Behavior (Logging, FluentValidation, Transaction)
- Global ExceptionHandler
- BackgroundService (Periodically Update Reservation Status)

## Follow-up Project
https://roadmap.sh/projects/movie-reservation-system