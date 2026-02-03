namespace Growment.API.Models
{
    public class UserOtp
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string OtpCode { get; set; }

        public string Purpose { get; set; }   // e.g. "CHANGE_PASSWORD"

        public DateTime ExpiryAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
