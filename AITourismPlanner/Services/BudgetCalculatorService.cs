using AITourismPlanner.Models;

namespace AITourismPlanner.Services
{
    public class BudgetCalculatorService
    {
        public BudgetBreakdown CalculateBudget(decimal totalBudget, int days, int travelers, string category)
        {
            var breakdown = new BudgetBreakdown
            {
                TotalBudget = totalBudget,
                Days = days,
                Travelers = travelers
            };

            // Standard percentage distribution
            breakdown.Accommodation = totalBudget * 0.35m;  // 35% for hotel
            breakdown.Food = totalBudget * 0.20m;           // 20% for food
            breakdown.Transport = totalBudget * 0.20m;      // 20% for transport
            breakdown.Activities = totalBudget * 0.15m;     // 15% for activities
            breakdown.Shopping = totalBudget * 0.10m;       // 10% for shopping

            // Adjust based on category
            if (category == "Adventure")
            {
                breakdown.Activities += breakdown.Activities * 0.3m;
                breakdown.Shopping -= breakdown.Shopping * 0.3m;
            }
            else if (category == "Honeymoon")
            {
                breakdown.Accommodation += breakdown.Accommodation * 0.2m;
                breakdown.Shopping += breakdown.Shopping * 0.2m;
            }
            else if (category == "Budget")
            {
                breakdown.Accommodation -= breakdown.Accommodation * 0.2m;
                breakdown.Food -= breakdown.Food * 0.1m;
            }

            // Daily breakdown
            breakdown.DailyBudget = totalBudget / days;
            breakdown.PerPersonBudget = totalBudget / travelers;

            return breakdown;
        }

        public string GetSavingsTip(BudgetBreakdown breakdown, string destination)
        {
            var tips = new List<string>();

            if (breakdown.Accommodation > breakdown.TotalBudget * 0.4m)
                tips.Add($"Consider budget hotels or hostels in {destination} to save on accommodation");

            if (breakdown.Transport > breakdown.TotalBudget * 0.25m)
                tips.Add("Use public transport instead of private taxis to reduce costs");

            if (breakdown.Food > breakdown.TotalBudget * 0.25m)
                tips.Add("Try local street food instead of expensive restaurants");

            if (!tips.Any())
                tips.Add($"Your budget allocation for {destination} looks good! Consider booking in advance for better deals");

            return string.Join("\n• ", tips);
        }
    }

    public class BudgetBreakdown
    {
        public decimal TotalBudget { get; set; }
        public int Days { get; set; }
        public int Travelers { get; set; }
        public decimal Accommodation { get; set; }
        public decimal Food { get; set; }
        public decimal Transport { get; set; }
        public decimal Activities { get; set; }
        public decimal Shopping { get; set; }
        public decimal DailyBudget { get; set; }
        public decimal PerPersonBudget { get; set; }
    }
}