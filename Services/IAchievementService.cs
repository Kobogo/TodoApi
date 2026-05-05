using System.Threading.Tasks;

namespace TodoApi.Services
{
    public interface IAchievementService
    {
        Task CheckAndAwardAchievementsAsync(int userId, string category);
    }
}