using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyChiTieu.Dtos;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly ChiTieuContext _context;
        private readonly IPasswordHasher<NguoiDung> _passwordHasher;
        private readonly IConfiguration _configuration;

        public NguoiDungController(
            ChiTieuContext context,
            IPasswordHasher<NguoiDung> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NguoiDungDto>>> GetAll()
        {
            var users = await _context.NguoiDungs
                .AsNoTracking()
                .Select(nd => new NguoiDungDto(nd.MaNguoiDung, nd.HoTen, nd.Email))
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NguoiDungDto>> GetById(int id)
        {
            var user = await _context.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(nd => nd.MaNguoiDung == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(ToDto(user));
        }

        [AllowAnonymous]
        [HttpPost("dangky")]
        public async Task<ActionResult<NguoiDungDto>> DangKy(CreateNguoiDungRequest request)
        {
            if (await _context.NguoiDungs.AnyAsync(x => x.Email == request.Email))
            {
                return BadRequest("Email đã tồn tại!");
            }

            var entity = new NguoiDung
            {
                HoTen = request.HoTen,
                Email = request.Email,
                MatKhau = string.Empty
            };

            entity.MatKhau = _passwordHasher.HashPassword(entity, request.MatKhau);

            _context.NguoiDungs.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.MaNguoiDung }, ToDto(entity));
        }

        [AllowAnonymous]
        [HttpPost("dangnhap")]
        public async Task<ActionResult<DangNhapResponseDto>> DangNhap([FromBody] DangNhapRequest request)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (user == null)
            {
                return Unauthorized("Sai email hoặc mật khẩu!");
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.MatKhau, request.MatKhau);
            if (verification == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Sai email hoặc mật khẩu!");
            }

            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.MatKhau = _passwordHasher.HashPassword(user, request.MatKhau);
                await _context.SaveChangesAsync();
            }

            return Ok(BuildLoginResponse(user));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateNguoiDungRequest request)
        {
            var entity = await _context.NguoiDungs.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (await _context.NguoiDungs.AnyAsync(u => u.Email == request.Email && u.MaNguoiDung != id))
            {
                return BadRequest("Email đã được sử dụng bởi người dùng khác");
            }

            entity.HoTen = request.HoTen;
            entity.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.MatKhau))
            {
                entity.MatKhau = _passwordHasher.HashPassword(entity, request.MatKhau);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.NguoiDungs.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.NguoiDungs.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private DangNhapResponseDto BuildLoginResponse(NguoiDung user)
        {
            var token = GenerateToken(user, out var expiresAt);
            return new DangNhapResponseDto(token, expiresAt, ToDto(user));
        }

        private string GenerateToken(NguoiDung user, out DateTime expiresAt)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key is not configured");
            var issuer = jwtSection.GetValue<string>("Issuer") ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
            var audience = jwtSection.GetValue<string>("Audience") ?? throw new InvalidOperationException("Jwt:Audience is not configured");
            var expiresMinutes = jwtSection.GetValue<int?>("ExpiresMinutes") ?? 60;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.MaNguoiDung.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static NguoiDungDto ToDto(NguoiDung entity) => new(entity.MaNguoiDung, entity.HoTen, entity.Email);
    }
}
