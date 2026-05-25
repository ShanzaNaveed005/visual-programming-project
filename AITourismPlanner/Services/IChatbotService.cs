using AITourismPlanner.Models;

namespace AITourismPlanner.Services
{
    public interface IChatbotService
    {
        Task<string> GetBotResponseAsync(string userMessage, int? userId = null);
        Task SaveChatHistoryAsync(int userId, string userMessage, string botResponse);
        Task<List<ChatbotHistory>> GetChatHistoryAsync(int userId);
    }
}