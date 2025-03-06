using Microsoft.AspNetCore.Mvc;

namespace apiCleaner.Controllers;

[ApiController]
[Route("[controller]")]
public class CleanerService : ControllerBase
{
    [HttpGet]
    [Route("/getdata")]
    public ActionResult<List<string>> GetData()
    {
        return Success;
    }
    
    [HttpGet]
    [Route("/removeheader")]
    public ActionResult<List<string>> RemoveData()
    {
        return Success;
    }

    [HttpPost]
    [Route("/forwarddata")]
    public ActionResult<List<string>> ForwardData()
    {
        return Success;
    }
}
