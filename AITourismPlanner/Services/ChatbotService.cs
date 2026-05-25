using AITourismPlanner.Data;
using AITourismPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace AITourismPlanner.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIRecommendationService _aiService;

        public ChatbotService(ApplicationDbContext context, IAIRecommendationService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<string> GetBotResponseAsync(string userMessage, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Please ask me something about travel!";

            string message = userMessage.ToLower();

            // Greetings
            if (message.Contains("hello") || message.Contains("hi") || message.Contains("assalam"))
            {
                return "Assalam-o-Alaikum! 👋 I'm your AI Travel Assistant. How can I help you plan your trip today?";
            }

            // Help
            if (message.Contains("help") || message.Contains("what can you do"))
            {
                return "I can help you with:\n" +
                       "📍 Finding best destinations\n" +
                       "💰 Budget-friendly recommendations\n" +
                       "🏨 Hotel suggestions\n" +
                       "✈️ Transport options\n" +
                       "📅 Trip planning\n" +
                       "🌤 Weather information\n\n" +
                       "Just tell me your budget or destination!";
            }

            // Budget-based recommendations
            if (message.Contains("budget") || message.Contains("under") || message.Contains("pkr"))
            {
                // Extract budget from message
                decimal budget = ExtractBudget(message);
                if (budget > 0)
                {
                    var destination = await _aiService.GetBestMatchAsync(budget, 3, "");
                    if (destination != null)
                    {
                        return $"🎯 Based on your budget of PKR {budget:N0}, I recommend visiting **{destination.name}**!\n\n" +
                               $"📍 Location: {destination.city}\n" +
                               $"💰 Estimated cost: PKR {destination.estimated_cost:N0}\n" +
                               $"⭐ Rating: {destination.rating_average}/5\n\n" +
                               $"Would you like me to plan a full itinerary for {destination.name}?";
                    }
                    else
                    {
                        return "I couldn't find a destination within your budget. Try increasing your budget or check out our special deals!";
                    }
                }
            }

            // Destination-specific queries
            if (message.Contains("hunza"))
            {
                return "🏔 **Hunza Valley** - A paradise on earth!\n\n" +
                       "✨ Highlights:\n" +
                       "• Attabad Lake\n" +
                       "• Karimabad Fort\n" +
                       "• Eagle's Nest viewpoint\n" +
                       "• Rakaposhi view point\n\n" +
                       "💰 Estimated cost: PKR 50,000 for 3 days\n" +
                       "🌤 Best season: Summer (May-September)\n\n" +
                       "Shall I book a trip for you?";
            }

            if (message.Contains("murree"))
            {
                return "🌲 **Murree** - The Queen of Hill Stations!\n\n" +
                       "✨ Highlights:\n" +
                       "• Mall Road\n" +
                       "• Kashmir Point\n" +
                       "• Pindi Point\n" +
                       "• Patriata (New Murree)\n\n" +
                       "💰 Estimated cost: PKR 25,000 for 2 days\n" +
                       "🌤 Best season: Winter (December-February) for snow\n\n" +
                       "Ready to book your Murree trip?";
            }

            if (message.Contains("skardu"))
            {
                return "🏔 **Skardu** - Gateway to the Giants!\n\n" +
                       "✨ Highlights:\n" +
                       "• Shangrila Resort (Lower Kachura Lake)\n" +
                       "• Katpana Cold Desert\n" +
                       "• Manthokha Waterfall\n" +
                       "• Sarfaranga Cold Desert\n\n" +
                       "💰 Estimated cost: PKR 70,000 for 4 days\n" +
                       "🌤 Best season: Summer (June-August)\n\n" +
                       "Want me to find hotels in Skardu?";
            }

            // Weather queries
            if (message.Contains("weather"))
            {
                return "🌤 To get weather information, please visit our Destinations page and select a destination. Each destination has detailed weather forecasts!";
            }

            // Hotels
            if (message.Contains("hotel"))
            {
                return "🏨 I can help you find hotels! Please:\n" +
                       "1. Go to Hotels page\n" +
                       "2. Select your destination\n" +
                       "3. Choose your dates\n" +
                       "4. Book instantly!\n\n" +
                       "Or tell me which city you're looking for hotels in.";
            }

            // Default response
            return "Thanks for your message! 🤖\n\n" +
                   "I can help you with:\n" +
                   "• Finding destinations (e.g., 'Show me Hunza')\n" +
                   "• Budget planning (e.g., 'Budget 50000')\n" +
                   "• Hotel recommendations\n" +
                   "• Trip planning\n\n" +
                   "What would you like to know?";
        }

        private decimal ExtractBudget(string message)
        {
            var words = message.Split(' ');
            foreach (var word in words)
            {
                if (decimal.TryParse(word, out decimal budget))
                {
                    return budget;
                }
            }
            return 0;
        }

        public async Task SaveChatHistoryAsync(int userId, string userMessage, string botResponse)
        {
            var chat = new ChatbotHistory
            {
                user_id = userId,
                user_message = userMessage,
                bot_response = botResponse,
                created_at = DateTime.Now
            };
            _context.chatbot_history.Add(chat);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatbotHistory>> GetChatHistoryAsync(int userId)
        {
            return await _context.chatbot_history
                .Where(c => c.user_id == userId)
                .OrderByDescending(c => c.created_at)
                .Take(10)
                .ToListAsync();
        }
    }
}