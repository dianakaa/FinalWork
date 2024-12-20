using ServiceLayer.Models;
using System.Net.Http.Json;

namespace ServiceLayer.Services
{
    public class ExamOrderProductService
    {
        private readonly HttpClient _client;
        private readonly string _baseAddress = "http://localhost:5114/api/";

        public ExamOrderProductService()
        {
            _client = new() { BaseAddress = new Uri(_baseAddress) };
        }

        public async Task<List<ExamOrderProduct>> GetProductsInOrder(int? orderId)
        {
            try
            {
                var orderProducts = await GetOrderProducts();
                return orderProducts.Where(op => op.OrderId == orderId).ToList();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<List<ExamOrderProduct>?> GetOrderProducts()
        {
            try
            {
                return await _client.GetFromJsonAsync<List<ExamOrderProduct>>("ExamOrderProducts");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> GetProductAmountInOrderWithArticle(int? orderId, string article)
        {
            try
            {
                var orderProducts = await GetOrderProducts();
                return orderProducts.Where(o => o.OrderId == orderId && o.ProductArticleNumber == article).Select(o => o.Amount).FirstOrDefault().ToString();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка по при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<decimal?> GetSumOrder(int? orderId)//получение суммы заказа
        {
            try
            {
                var response = await _client.GetAsync($"ExamOrderProducts/{orderId}/summ");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<decimal?>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка получения общей стоимости при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<decimal?> GetDiscountOrder(int? orderId)//получение скидки заказа
        {
            try
            {
                var response = await _client.GetAsync($"ExamOrderProducts/{orderId}/discount");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<decimal?>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка получения общей скидки при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddOrderProductAsync(int? orderID, string productArticleNumber, int amount)//создание новых записей о товарах в новом заказе
        {
            try
            {
                var orderProduct = new ExamOrderProduct() { OrderId = orderID, ProductArticleNumber = productArticleNumber, Amount = (short)amount };
                var response = await _client.PostAsJsonAsync("ExamOrderProducts", orderProduct);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка добавления записей при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
