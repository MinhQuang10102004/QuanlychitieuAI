namespace QuanLyChiTieu.Dtos
{
    public record NguoiDungDto(int MaNguoiDung, string HoTen, string Email);

    public record CreateNguoiDungRequest(string HoTen, string Email, string MatKhau);

    public record UpdateNguoiDungRequest(string HoTen, string Email, string? MatKhau);

    public record DangNhapRequest(string Email, string MatKhau);

    public record DangNhapResponseDto(string Token, DateTime ExpiresAt, NguoiDungDto User);
}
