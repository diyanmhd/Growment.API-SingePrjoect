using Dapper;
using Growment.API.Data;
using Growment.API.Models.Mentor;
using System.Data;

namespace Growment.API.Repositories.Mentor
{
    public class MentorRepository : IMentorRepository
    {
        private readonly DapperContext _context;

        public MentorRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task ApplyForMentorAsync(int userId, ApplyMentorRequest request)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@PrimarySkill", request.PrimarySkill);
            parameters.Add("@SubSkills", request.SubSkills ?? "");
            parameters.Add("@ExperienceYears", request.ExperienceYears);
            parameters.Add("@Background", request.Background);
            parameters.Add("@MentoringExperience", request.MentoringExperience);
            parameters.Add("@ProfileLinks", request.ProfileLinks);

            await connection.ExecuteAsync(
                "sp_ApplyForMentor",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<MentorApplicationResponse>> GetPendingApplicationsAsync()
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryAsync<MentorApplicationResponse>(
                "sp_GetPendingMentorApplications",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task ReviewApplicationAsync(int applicationId, string status, int adminUserId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ApplicationId", applicationId);
            parameters.Add("@Status", status);
            parameters.Add("@AdminUserId", adminUserId);

            await connection.ExecuteAsync(
                "sp_ReviewMentorApplication",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
