using api.models;
using infrastructure;

namespace service;

public class SearchingService
{
    private SearchingRepo _searchingRepo;

    public SearchingService(SearchingRepo searchingRepo)
    {
        _searchingRepo = searchingRepo;
    }
    public async Task<List<Email>> GetEmailsWithSerarchterm(string searchTerm)
    {
        return await _searchingRepo.GetEmailsWithSerarchterm(searchTerm);
    }
}
