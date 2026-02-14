namespace Growment.API.Models.Mentor
{
    public class ApplyMentorRequest
    {
        public string PrimarySkill { get; set; }
        public string SubSkills { get; set; }
        public int ExperienceYears { get; set; }
        public string Background { get; set; }
        public bool MentoringExperience { get; set; }
        public string ProfileLinks { get; set; }
    }
}
