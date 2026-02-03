namespace Growment.API.Models
{
    public class OtpChangePasswordRequest
    {
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }
}
