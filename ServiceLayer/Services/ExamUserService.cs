using ServiceLayer.Models;
using System.Net.Http.Json;

namespace ServiceLayer.Services
{
    public class ExamUserService
    {
        private readonly HttpClient _client;
        private readonly string _baseAddress = "http://localhost:5114/api/";

        public ExamUserService()
        {
            _client = new() { BaseAddress = new Uri(_baseAddress) };
        }
        public async Task<ExamUser?> GetUserByLoginAndPasswordAsync(string login, string password)
        {
            try
            {
                var users = await _client.GetFromJsonAsync<IEnumerable<ExamUser>>("ExamUsers");
                return users.FirstOrDefault(u => u.UserLogin == login && u.UserPassword == password);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка получения данных пользователя при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string?> GetUserFullNameWithOrderIdAsync(int? orderId)
        {
            try
            {
                var response = await _client.GetAsync($"ExamOrders/{orderId}");
                response.EnsureSuccessStatusCode();
                var order = await response.Content.ReadFromJsonAsync<ExamOrder>();

                if (order.UserId != null)
                {
                    response = await _client.GetAsync($"ExamUsers/{order?.UserId}");
                    response.EnsureSuccessStatusCode();
                    var user = await response.Content.ReadFromJsonAsync<ExamUser>();

                    return $"{user.UserSurname} {user.UserName} {user.UserPatronymic}";
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Ошибка получения данных пользователя при выполнении запроса к API: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
