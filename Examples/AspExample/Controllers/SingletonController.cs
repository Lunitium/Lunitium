using AspExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspExample.Controllers;

[ApiController]
[Route("singleton")]
public class SingletonController(SingletonService singletonService) : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(singletonService.Counter);
    }
}