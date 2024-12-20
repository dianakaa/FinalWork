using ExamWebApi.Data;
using ExamWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamPickupPointsController : ControllerBase
    {
        private readonly ExamContext _context;

        public ExamPickupPointsController(ExamContext context)
        {
            _context = context;
        }

        // GET: api/ExamPickupPoints
        [HttpGet]
        public async Task<ActionResult<List<ExamPickupPoint>>> GetExamPickupPoints()
        {
            return await _context.ExamPickupPoints.ToListAsync();
        }
    }
}
