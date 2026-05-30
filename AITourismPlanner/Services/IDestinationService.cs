using AITourismPlanner.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AITourismPlanner.Services
{
    public interface IDestinationService
    {
        Task<List<DestinationApiModel>> GetAllDestinationsAsync();
        Task<List<DestinationApiModel>> GetDestinationsByCategoryAsync(string category);
        Task<DestinationApiModel> GetDestinationDetailAsync(string name);
        Task<List<DestinationApiModel>> SearchDestinationsAsync(string query);
        Task<List<string>> GetAutocompleteSuggestionsAsync(string term);
    }
}