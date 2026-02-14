namespace Growment.API.Models.Mentor
{
    public class MentorApplicationResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PrimarySkill { get; set; }
        public string SubSkills { get; set; }
        public int ExperienceYears { get; set; }
        public string Background { get; set; }
        public bool MentoringExperience { get; set; }
        public string ProfileLinks { get; set; }

        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
