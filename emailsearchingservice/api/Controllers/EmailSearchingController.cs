using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailSearchingController : ControllerBase
{

    [HttpGet]
    [Route("/getEmailSearch/{searchTerm}")]
    public ResponseDto GetEmailSearch(string searchTerm)
    {
        return new ResponseDto();
    }

    [HttpGet]
    [Route("/downloadEmails/{downloadTerm}")]
    public ResponseDto downloadEmails(string downloadTerm)
    {
        return new ResponseDto();
    }
}
