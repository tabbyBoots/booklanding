using System;

namespace mvcDapper3.Models.ViewModel
{
    public class vmPasswordResetTokenInfo
    {
        public string Token { get; set; }
        public DateTime CreationTime { get; set; }
        public string Email { get; set; } // To associate token with email

        // Parameterless constructor required for session deserialization
        public vmPasswordResetTokenInfo() {}

        public vmPasswordResetTokenInfo(string token, string email)
        {
            Token = token;
            Email = email;
            CreationTime = DateTime.UtcNow;
        }

        public bool IsExpired(int minutesToExpire)
        {
            return DateTime.UtcNow > CreationTime.AddMinutes(minutesToExpire);
        }
    }
}
