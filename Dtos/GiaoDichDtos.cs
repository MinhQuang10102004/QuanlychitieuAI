namespace QuanLyChiTieu.Dtos
{
    public record GiaoDichDto(
        int MaGiaoDich,
        int MaNguoiDung,
        string HoTenNguoiDung,
        int MaDanhMuc,
        string TenDanhMuc,
        decimal SoTien,
        DateTime NgayGiaoDich,
        string? MoTa);

    public record CreateGiaoDichRequest(
        int MaNguoiDung,
        int MaDanhMuc,
        decimal SoTien,
        DateTime NgayGiaoDich,
        string? MoTa);

    public record UpdateGiaoDichRequest(
        int MaNguoiDung,
        int MaDanhMuc,
        decimal SoTien,
        DateTime NgayGiaoDich,
        string? MoTa);
}
