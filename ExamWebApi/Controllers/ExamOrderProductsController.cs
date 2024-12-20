using ExamWebApi.Data;
using ExamWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamOrderProductsController : ControllerBase
    {
        private readonly ExamContext _context;

        public ExamOrderProductsController(ExamContext context)
        {
            _context = context;
        }

        // GET: api/ExamOrderProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamOrderProduct>>> GetExamOrderProducts()
        {
            return await _context.ExamOrderProducts.ToListAsync();
        }

        // GET: api/ExamOrderProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamOrderProduct>> GetExamOrderProduct(int id)
        {
            var examOrderProduct = await _context.ExamOrderProducts.FindAsync(id);

            if (examOrderProduct == null)
            {
                return NotFound();
            }

            return examOrderProduct;
        }


        // GET: api/ExamOrderProducts/5/summ
        [HttpGet("{orderId}/summ")]
        public ActionResult<decimal?> GetSumOrder(int orderId)//получение суммы заказа
        {
            try
            {
                if (!ExamOrderProductExists(orderId))
                    return NotFound();
                return _context.ExamOrderProducts
                    .Include(eop => eop.ProductArticleNumberNavigation) // Включаем связанные ExamProducts
                    .Include(eop => eop.Order) // Включаем связанные ExamOrders
                    .Where(eop => eop.Order.OrderId == orderId)
                    .Select(eop => new
                    {
                        Cost = eop.ProductArticleNumberNavigation.ProductCost * (100 - eop.ProductArticleNumberNavigation.ProductDiscountAmount) / 100 * eop.Amount
                    })
                    .Sum(x => x.Cost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера: " + ex.Message);
            }
        }

        // GET: api/ExamOrderProducts/5/discount
        [HttpGet("{orderId}/discount")]
        public ActionResult<decimal?> GetDiscountOrder(int orderId)//получение скидки заказа
        {
            try
            {
                if (!ExamOrderProductExists(orderId))
                    return NotFound();
                return _context.ExamOrderProducts
                    .Include(eop => eop.ProductArticleNumberNavigation) // Включаем связанные ExamProducts
                    .Include(eop => eop.Order) // Включаем связанные ExamOrders
                    .Where(eop => eop.Order.OrderId == orderId)
                    .Select(eop => new
                    {
                        Discount = (eop.ProductArticleNumberNavigation.ProductCost - eop.ProductArticleNumberNavigation.ProductCost * (100 - eop.ProductArticleNumberNavigation.ProductDiscountAmount) / 100) * eop.Amount
                    })
                    .Sum(x => x.Discount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера: " + ex.Message);
            }
        }

        // PUT: api/ExamOrderProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExamOrderProduct(int id, ExamOrderProduct examOrderProduct)
        {
            if (id != examOrderProduct.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(examOrderProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamOrderProductExists(id))
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

        // POST: api/ExamOrderProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ExamOrderProduct>> PostExamOrderProduct(ExamOrderProduct examOrderProduct)
        {
            _context.ExamOrderProducts.Add(examOrderProduct);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ExamOrderProductExists(examOrderProduct.OrderId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetExamOrderProduct", new { id = examOrderProduct.OrderId }, examOrderProduct);
        }

        // DELETE: api/ExamOrderProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExamOrderProduct(int id)
        {
            var examOrderProduct = await _context.ExamOrderProducts.FindAsync(id);
            if (examOrderProduct == null)
            {
                return NotFound();
            }

            _context.ExamOrderProducts.Remove(examOrderProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExamOrderProductExists(int? id)
        {
            return _context.ExamOrderProducts.Any(e => e.OrderId == id);
        }
    }
}
