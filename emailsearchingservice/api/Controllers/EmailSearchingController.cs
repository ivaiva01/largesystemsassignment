using api.models;
using Microsoft.AspNetCore.Mvc;
using service;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailSearchingController : ControllerBase
{
    private SearchingService _searchingService;

    public EmailSearchingController(SearchingService searchingService)
    {
        _searchingService = searchingService;
    }
    
    [HttpGet]
    [Route("/getEmailSearch/{searchTerm}")]
    public ResponseDto GetEmailSearch(string searchTerm)
    {
        List<Email> allLayerElements;
        allLayerElements = _searchingService.GetEmailsWithSerarchterm(searchTerm);

        return new ResponseDto
        {
            MessageToClient = "All email with matching results",
            ResponseData = null
        };
    }

    [HttpGet]
    [Route("/downloadEmails/{downloadTerm}")]
    public ResponseDto downloadEmails(string downloadTerm)
    {
        return new ResponseDto();
    }
}
