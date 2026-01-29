namespace Growment.API.Models
{
    public class UserRegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }   // ✅ CHANGE HERE
        public string PhoneNumber { get; set; }
    }
}
