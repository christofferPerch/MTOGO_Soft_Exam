namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class OperatingHoursDto
    {
        public DayEnum Day { get; set; }
        public TimeSpan OpeningHours { get; set; }
        public TimeSpan ClosingHours { get; set; }
    }

    public enum DayEnum
    {
        Undefined = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }
}
