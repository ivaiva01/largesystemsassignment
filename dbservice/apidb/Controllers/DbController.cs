using Microsoft.AspNetCore.Mvc;

namespace apidb.Controllers;

[ApiController]
[Route("[controller]")]
public class DbController : ControllerBase
{
    
    [HttpPost]
    [Route("/savedata")]
    public async Task<IActionResult> Save([FromBody] Email email)
    {
        return Created;
    }

    [HttpGet]
    [Route("/getdata/{id}")]

    public async ResponseDto Get(string id)
    {
        return Created;
    }

    [HttpGet]
    [Route("/getalldata/{searchTerm}")]

    public async ResponseDto GetAll(string searchTerm)
    {
        return Created;
    }
}
