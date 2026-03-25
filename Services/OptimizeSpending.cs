namespace QuanLyChiTieu.Services;

public class OptimizeSpending
{
    public string Suggest(decimal currentSpending)
    {
        if (currentSpending <= 0)
        {
            return "Chưa có dữ liệu chi tiêu để tối ưu.";
        }

        if (currentSpending > 1_000_000m)
        {
            return "Bạn có thể tiết kiệm 20% chi phí nếu rà soát các danh mục không thiết yếu.";
        }

        return "Chi tiêu của bạn đang ở mức hợp lý.";
    }
}
