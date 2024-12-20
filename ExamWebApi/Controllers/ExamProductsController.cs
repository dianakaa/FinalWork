using ExamWebApi.Data;
using ExamWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.DTOs;

namespace ExamWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamProductsController : ControllerBase
    {
        private readonly ExamContext _context;

        public ExamProductsController(ExamContext context)
        {
            _context = context;
        }
       
        // GET: api/ExamProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsAsync()//получение списка продуктов из БД с фильтрацией
        {
            try
            {
                return await _context.ExamProducts
                    .Select(prod => new ProductDTO
                    {
                        ProductName = prod.ProductName,
                        ProductArticleNumber = prod.ProductArticleNumber,
                        ProductCategory = prod.ProductCategory,
                        ProductPhoto = prod.ProductPhoto,
                        ProductStatus = prod.ProductStatus,
                        ProductDescription = prod.ProductDescription,
                        ProductCost = prod.ProductCost,
                        ProductDiscountAmount = prod.ProductDiscountAmount,
                        ProductManufacturer = prod.Manufacturer.Name,
                        ProductQuantityInStock = prod.ProductQuantityInStock,
                        TotalCost = prod.ProductDiscountAmount.HasValue ? prod.ProductCost * (100 - prod.ProductDiscountAmount.Value) / 100 : prod.ProductCost
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера: " + ex.Message);
            }
        }

        // GET: api/ExamProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamProduct>> GetExamProduct(string id)
        {
            var examProduct = await _context.ExamProducts.FindAsync(id);

            if (examProduct == null)
            {
                return NotFound();
            }

            return examProduct;
        }

        // PUT: api/ExamProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExamProduct(string id, ExamProduct examProduct)
        {
            if (id != examProduct.ProductArticleNumber)
            {
                return BadRequest();
            }

            _context.Entry(examProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamProductExists(id))
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

        // POST: api/ExamProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ExamProduct>> PostExamProduct(ExamProduct examProduct)
        {
            _context.ExamProducts.Add(examProduct);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ExamProductExists(examProduct.ProductArticleNumber))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetExamProduct", new { id = examProduct.ProductArticleNumber }, examProduct);
        }

        // DELETE: api/ExamProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExamProduct(string id)
        {
            var examProduct = await _context.ExamProducts.FindAsync(id);
            if (examProduct == null)
            {
                return NotFound();
            }

            _context.ExamProducts.Remove(examProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExamProductExists(string id)
        {
            return _context.ExamProducts.Any(e => e.ProductArticleNumber == id);
        }
    }
}
