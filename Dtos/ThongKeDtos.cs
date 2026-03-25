namespace QuanLyChiTieu.Dtos
{
    public record MonthlySummaryDto(int Month, decimal TotalAmount, int TransactionCount);

    public record CategorySummaryDto(int MaDanhMuc, string TenDanhMuc, decimal TotalAmount, int TransactionCount);
}
