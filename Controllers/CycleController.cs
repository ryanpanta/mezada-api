using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.User;
using WebApiMezada.Middleware.Attributes;
using WebApiMezada.Models;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.User;

namespace WebApiMezada.Controllers
{
    [Route("api/Cycles")]
    [ApiController]
    public class CycleController : Controller
    {
        private readonly ICycleService _cycleService;
        public CycleController(ICycleService cycleService)
        {
            _cycleService = cycleService;
        }
    }
}
