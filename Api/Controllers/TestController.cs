using Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public TestController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            appDbContext.Database.EnsureDeleted();
            appDbContext.Database.EnsureCreated();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_appDbContext.Users.FirstOrDefault());
        }
    }
}
