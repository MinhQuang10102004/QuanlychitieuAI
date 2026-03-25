namespace QuanLyChiTieu.Dtos
{
    public record DanhMucDto(int MaDanhMuc, string TenDanhMuc);

    public record CreateDanhMucRequest(string TenDanhMuc);

    public record UpdateDanhMucRequest(string TenDanhMuc);
}
