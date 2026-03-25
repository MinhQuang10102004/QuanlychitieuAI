using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services
{
    public class InvitationService
    {
        private readonly ChiTieuContext _context;
        private readonly ILogger<InvitationService> _logger;
        private readonly IConfiguration _config;

        public InvitationService(ChiTieuContext context, ILogger<InvitationService> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task<string> CreateInvitationAsync(int inviterId, int householdId, string? email, string? phone, TimeSpan? validFor = null)
        {
            var token = GenerateToken();
            var inv = new Invitation
            {
                Email = email,
                Phone = phone,
                Token = token,
                HouseholdId = householdId,
                InviterId = inviterId,
                ExpiresAt = DateTime.UtcNow.Add(validFor ?? TimeSpan.FromDays(7)),
                Accepted = false
            };

            _context.Invitations.Add(inv);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            // Try to send email if SMTP configured
            try
            {
                var smtpHost = _config["Smtp:Host"];
                if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(smtpHost))
                {
                    var smtpPort = int.TryParse(_config["Smtp:Port"], out var p) ? p : 25;
                    var smtpUser = _config["Smtp:User"];
                    var smtpPass = _config["Smtp:Pass"];
                    var from = _config["Smtp:From"] ?? smtpUser ?? "no-reply@localhost";

                    using var msg = new System.Net.Mail.MailMessage();
                    msg.From = new System.Net.Mail.MailAddress(from);
                    msg.To.Add(new System.Net.Mail.MailAddress(email));
                    msg.Subject = "Lời mời tham gia hộ gia đình";
                    var baseUrl = _config["App:BaseUrl"] ?? "";
                    var registerUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/Account/Register?inviteToken={token}" : ($"{baseUrl.TrimEnd('/')}/Account/Register?inviteToken={token}");
                    msg.Body = $"Bạn được mời tham gia hộ gia đình. Sử dụng mã sau để tham gia: {token}\nHoặc mở liên kết: {registerUrl}";

                    using var smtp = new System.Net.Mail.SmtpClient(smtpHost, smtpPort);
                    smtp.EnableSsl = bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) && ssl;
                    if (!string.IsNullOrWhiteSpace(smtpUser)) smtp.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                    await smtp.SendMailAsync(msg).ConfigureAwait(false);
                    _logger.LogInformation("Sent invitation email to {Email}", email);
                }
                else if (!string.IsNullOrWhiteSpace(phone))
                {
                    // SMS provider not configured — log token for manual delivery
                    _logger.LogInformation("Invitation for phone {Phone}: {Token}", phone, token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send invitation notification (email/SMS). Token: {Token}", token);
            }

            _logger.LogInformation("Created invitation {Token} for household {Household} by {Inviter}", token, householdId, inviterId);
            return token;
        }

        public async Task<bool> AcceptInvitationAsync(string token, int userId)
        {
            var inv = await _context.Invitations.FirstOrDefaultAsync(i => i.Token == token && !i.Accepted && i.ExpiresAt > DateTime.UtcNow);
            if (inv == null) return false;

            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return false;

            user.MaHoGiaDinh = inv.HouseholdId;
            user.AccountType = "Household";
            inv.Accepted = true;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogInformation("User {User} accepted invitation {Token} to household {Household}", userId, token, inv.HouseholdId);
            return true;
        }

        public async Task<IEnumerable<Invitation>> GetPendingInvitationsForUserAsync(int userId)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return Enumerable.Empty<Invitation>();

            return await _context.Invitations
                .Where(i => !i.Accepted && i.ExpiresAt > DateTime.UtcNow &&
                       ((i.Email != null && i.Email == user.Email) || (i.Phone != null && i.Phone == user.Phone)))
                .OrderBy(i => i.ExpiresAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invitation>> GetSentInvitationsByUserAsync(int userId)
        {
            return await _context.Invitations
                .Where(i => !i.Accepted && i.InviterId == userId)
                .OrderByDescending(i => i.ExpiresAt)
                .ToListAsync();
        }

        public async Task<bool> CancelInvitationAsync(string token, int userId)
        {
            var inv = await _context.Invitations.FirstOrDefaultAsync(i => i.Token == token && !i.Accepted);
            if (inv == null) return false;

            // Allow cancel if the requester is the inviter or is the household owner
            if (inv.InviterId == userId)
            {
                _context.Invitations.Remove(inv);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation("Invitation {Token} canceled by inviter {User}", token, userId);
                return true;
            }

            // check household owner
            var household = await _context.HoGiaDinhs.FindAsync(inv.HouseholdId);
            if (household != null && household.ChuHoId == userId)
            {
                _context.Invitations.Remove(inv);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation("Invitation {Token} canceled by household owner {User}", token, userId);
                return true;
            }

            return false;
        }

        private static string GenerateToken()
        {
            var bytes = new byte[16];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
