using ExamWebApi.Data;
using ExamWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.DTOs;

namespace ExamWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamOrdersController : ControllerBase
    {
        private readonly ExamContext _context;

        public ExamOrdersController(ExamContext context)
        {
            _context = context;
        }

        // GET: api/ExamOrders
        [HttpGet]
        public async Task<ActionResult<List<ExamOrder>>> GetExamOrders()
        {
            return await _context.ExamOrders.ToListAsync();
        }

        // GET: api/ExamOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamOrder>> GetOrderByIdAsync(int id)
        {
            var order = await _context.ExamOrders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/ExamOrders/5/TotalCost
        [HttpGet("{id}/TotalCost")]
        public async Task<ActionResult<OrderSummaryDTO>> GetOrderByIdWithTotalCostAsync(int id)//получение заказа из БД по id
        {
            try
            {
                var order = await _context.ExamOrders.Include(p => p.ExamOrderProducts).ThenInclude(op => op.ProductArticleNumberNavigation)
                .Select(g => new OrderSummaryDTO
                {
                    OrderID = g.OrderId,
                    UserID = g.UserId,
                    OrderStatus = g.OrderStatus,
                    OrderDate = g.OrderDate,
                    OrderDeliveryDate = g.OrderDeliveryDate,
                    OrderPickupPoint = g.OrderPickupPoint,
                    OrderPickupCode = g.OrderPickupCode,
                    TotalCost = g.ExamOrderProducts
                        .Sum(op => op.ProductArticleNumberNavigation.ProductCost * (100 - op.ProductArticleNumberNavigation.ProductDiscountAmount) / 100 * op.Amount)
                }).FirstOrDefaultAsync(g => g.OrderID == id);

                if (order == null)
                {
                    return NotFound();
                }

                return order;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера: " + ex.Message);
            }
        }

        // GET: api/ExamOrders/TotalCost
        [HttpGet("TotalCost")]
        public async Task<ActionResult<List<OrderSummaryDTO>>> GetOrdersAsync()//получение заказов
        {
            try
            {
                var orders = await _context.ExamOrders.Include(p => p.ExamOrderProducts).ThenInclude(op => op.ProductArticleNumberNavigation)
                .Select(g => new OrderSummaryDTO
                {
                    OrderID = g.OrderId,
                    UserID = g.UserId,
                    OrderStatus = g.OrderStatus,
                    OrderDate = g.OrderDate,
                    OrderDeliveryDate = g.OrderDeliveryDate,
                    OrderPickupPoint = g.OrderPickupPoint,
                    OrderPickupCode = g.OrderPickupCode,
                    TotalCost = g.ExamOrderProducts
                        .Sum(op => op.ProductArticleNumberNavigation.ProductCost * (100 - op.ProductArticleNumberNavigation.ProductDiscountAmount) / 100 * op.Amount)
                }).ToListAsync();

                if (orders == null)
                {
                    return NotFound();
                }
                return orders;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера: " + ex.Message);
            }
        }

        // PUT: api/ExamOrders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{orderId}")]
        public async Task<IActionResult> PutExamOrder(int orderId, ExamOrder examOrder)
        {
            if (orderId != examOrder.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(examOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamOrderExists(orderId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ExamOrders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostExamOrder(ExamOrder examOrder)
        {
            _context.ExamOrders.Add(examOrder);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        private bool ExamOrderExists(int? id)
        {
            return _context.ExamOrders.Any(e => e.OrderId == id);
        }
    }
}
