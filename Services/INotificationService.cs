using System.Threading.Tasks;

namespace QuanLyChiTieu.Services
{
    public interface INotificationService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendSmsAsync(string toPhone, string message);
    }
}
