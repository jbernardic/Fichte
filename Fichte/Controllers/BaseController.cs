using Fichte.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fichte.Controllers
{
    [ApiController]
    public class BaseController(IConfiguration configuration, DatabaseContext context) : Controller
    {
        protected readonly IConfiguration _configuration = configuration;
        protected readonly DatabaseContext _context = context;
    }
}
