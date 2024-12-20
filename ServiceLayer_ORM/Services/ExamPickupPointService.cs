using Microsoft.EntityFrameworkCore;
using ServiceLayer_ORM.Data;
using ServiceLayer_ORM.Models;

namespace ServiceLayer_ORM.Services
{
    public class ExamPickupPointService
    {
        private readonly ExamContext _context = new();

        public async Task<List<ExamPickupPoint>> GetPickupPointsAsync()//получение списка пунктов выдачи из БД
        {
            return await _context.ExamPickupPoints.ToListAsync();
        }
    }
}
