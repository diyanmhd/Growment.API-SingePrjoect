using Growment.API.Models.Mentor;
using Growment.API.Repositories.Mentor;

namespace Growment.API.Services.Mentor
{
    public class MentorService : IMentorService
    {
        private readonly IMentorRepository _mentorRepository;

        public MentorService(IMentorRepository mentorRepository)
        {
            _mentorRepository = mentorRepository;
        }

        public async Task ApplyForMentorAsync(int userId, ApplyMentorRequest request)
        {
            // Example: Business validation can be added here
            if (string.IsNullOrWhiteSpace(request.PrimarySkill))
                throw new Exception("Primary skill is required.");

            await _mentorRepository.ApplyForMentorAsync(userId, request);
        }

        public async Task<List<MentorApplicationResponse>> GetPendingApplicationsAsync()
        {
            return await _mentorRepository.GetPendingApplicationsAsync();
        }

        public async Task ReviewApplicationAsync(int applicationId, string status, int adminUserId)
        {
            if (status != "Approved" && status != "Rejected")
                throw new Exception("Invalid status value");

            await _mentorRepository.ReviewApplicationAsync(applicationId, status, adminUserId);
        }
    }
}
