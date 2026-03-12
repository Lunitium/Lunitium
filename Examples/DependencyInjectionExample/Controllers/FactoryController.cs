using DependencyInjectionExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace DependencyInjectionExample.Controllers;

[ApiController]
[Route("factory")]
public class FactoryController(FactoryService factoryService) : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(factoryService.Date);
    }
}