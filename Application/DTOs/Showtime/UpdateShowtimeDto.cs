namespace MovieReservationSystemAPI.Application;

public class UpdateShowtimeDto {
    //public int Id {get; set;}
    
    public DateTime StartTime {get; set;}
    public decimal Price {get; set;}

    public int HallId {get; set;}
    public int MovieId {get; set;}

}