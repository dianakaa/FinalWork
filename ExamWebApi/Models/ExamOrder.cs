using System.Text.Json.Serialization;

namespace ExamWebApi.Models;

public partial class ExamOrder
{
    public int? OrderId { get; set; }

    public int? UserId { get; set; }

    public string OrderStatus { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime OrderDeliveryDate { get; set; }

    public int OrderPickupPoint { get; set; }

    public int OrderPickupCode { get; set; }

    [JsonIgnore]
    public virtual ICollection<ExamOrderProduct> ExamOrderProducts { get; set; } = new List<ExamOrderProduct>();

    [JsonIgnore]
    public virtual ExamPickupPoint? OrderPickupPointNavigation { get; set; }

    [JsonIgnore]
    public virtual ExamUser? User { get; set; }
}
