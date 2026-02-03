using Growment.API.Models.Mentor;

namespace Growment.API.Services.Mentor
{
    public interface IMentorService
    {
        Task ApplyForMentorAsync(int userId, ApplyMentorRequest request);

        Task<List<MentorApplicationResponse>> GetPendingApplicationsAsync();

        Task ReviewApplicationAsync(int applicationId, string status, int adminUserId);
    }
}
