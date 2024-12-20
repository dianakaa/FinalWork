using Microsoft.EntityFrameworkCore;
using ServiceLayer_ORM.Data;
using ServiceLayer_ORM.Models;

namespace ServiceLayer_ORM.Services
{
    public class ExamOrderProductService
    {
        public static readonly ExamContext _context = new();
        public async Task<List<ExamOrderProduct>> GetProductsInOrder(int orderId)
        {
            return await _context.ExamOrderProducts.Where(o => o.OrderId == orderId).ToListAsync();
        }

        public async Task<string> GetProductAmountInOrderWithArticle(int orderId, string article)
        {
            var amount = await _context.ExamOrderProducts.Where(o => o.OrderId == orderId && o.ProductArticleNumber == article).Select(o => o.Amount).FirstOrDefaultAsync();
            return amount.ToString();
        }


        public decimal? GetSumOrder(int orderId)//получение суммы заказа
        {
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

        public decimal? GetDiscountOrder(int orderId)//получение скидки заказа
        {
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

        public async Task AddOrderProductAsync(int orderID, string productArticleNumber, int amount)//создание новых записей о товарах в новом заказе
        {
            var orderProduct = new ExamOrderProduct() { OrderId = orderID, ProductArticleNumber = productArticleNumber, Amount = (short)amount };
            await _context.ExamOrderProducts.AddAsync(orderProduct);
            await _context.SaveChangesAsync();
        }
    }
}
