using Microsoft.AspNetCore.Identity;

namespace MTOGO.Services.AuthAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ApplicationUser user &&
                   Id == user.Id &&
                   UserName == user.UserName &&
                   NormalizedUserName == user.NormalizedUserName &&
                   Email == user.Email &&
                   NormalizedEmail == user.NormalizedEmail &&
                   EmailConfirmed == user.EmailConfirmed &&
                   PasswordHash == user.PasswordHash &&
                   SecurityStamp == user.SecurityStamp &&
                   ConcurrencyStamp == user.ConcurrencyStamp &&
                   PhoneNumber == user.PhoneNumber &&
                   PhoneNumberConfirmed == user.PhoneNumberConfirmed &&
                   TwoFactorEnabled == user.TwoFactorEnabled &&
                   EqualityComparer<DateTimeOffset?>.Default.Equals(LockoutEnd, user.LockoutEnd) &&
                   LockoutEnabled == user.LockoutEnabled &&
                   AccessFailedCount == user.AccessFailedCount &&
                   FirstName == user.FirstName &&
                   LastName == user.LastName &&
                   Address == user.Address &&
                   City == user.City &&
                   ZipCode == user.ZipCode &&
                   Country == user.Country;
        }
    }
}
