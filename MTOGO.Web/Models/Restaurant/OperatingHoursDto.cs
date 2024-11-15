namespace MTOGO.Web.Models.Restaurant
{
    public class OperatingHoursDto
    {
        public DayEnum Day { get; set; }
        public TimeSpan OpeningHours { get; set; }
        public TimeSpan ClosingHours { get; set; }
    }
}
