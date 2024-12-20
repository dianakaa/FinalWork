namespace ExamWebApi.Models;

public partial class ExamRole
{
    public byte RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<ExamUser> ExamUsers { get; set; } = new List<ExamUser>();
}
