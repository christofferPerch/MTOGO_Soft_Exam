namespace MTOGO.Services.AuthAPI.Models.Dto
{
    public class UpdateProfileDto
    {
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
