using Growment.API.Models.Mentor;

namespace Growment.API.Repositories.Mentor
{
    public interface IMentorRepository
    {
        Task ApplyForMentorAsync(int userId, ApplyMentorRequest request);

        Task<List<MentorApplicationResponse>> GetPendingApplicationsAsync();

        Task ReviewApplicationAsync(int applicationId, string status, int adminUserId);
    }
}
