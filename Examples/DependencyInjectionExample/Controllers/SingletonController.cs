using DependencyInjectionExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace DependencyInjectionExample.Controllers;

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