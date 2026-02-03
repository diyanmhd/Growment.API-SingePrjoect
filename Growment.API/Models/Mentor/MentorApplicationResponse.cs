namespace Growment.API.Models.Mentor
{
    public class MentorApplicationResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Skills { get; set; }
        public string Experience { get; set; }

        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
