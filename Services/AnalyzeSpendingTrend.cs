using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services;

public class AnalyzeSpendingTrend
{
    public string GetTrendSummary(IEnumerable<ExpensePredictionModel> data)
    {
        if (data == null)
        {
            return "Chưa có dữ liệu chi tiêu.";
        }

        var monthlyExpenses = data
            .GroupBy(e => (int)Math.Round(e.Month))
            .Select(g => new
            {
                Month = g.Key,
                TotalExpense = g.Sum(x => (decimal)x.Expense)
            })
            .OrderBy(g => g.Month)
            .ToList();

        if (monthlyExpenses.Count < 2)
        {
            return "Chưa đủ dữ liệu để so sánh xu hướng chi tiêu.";
        }

        var current = monthlyExpenses[^1];
        var previous = monthlyExpenses[^2];

        if (current.TotalExpense == previous.TotalExpense)
        {
            return "Chi tiêu tháng này tương đương tháng trước.";
        }

        if (previous.TotalExpense == 0)
        {
            return current.TotalExpense == 0
                ? "Chi tiêu vẫn chưa phát sinh." 
                : "Chi tiêu tháng này tăng mạnh so với tháng trước.";
        }

        var delta = current.TotalExpense - previous.TotalExpense;
        var percentChange = Math.Abs(previous.TotalExpense) < 0.01m
            ? 0m
            : (delta / previous.TotalExpense) * 100m;

        if (delta > 0)
        {
            return $"Chi tiêu tháng này tăng {percentChange:0.##}% so với tháng trước. Hãy xem xét điều chỉnh kế hoạch.";
        }

        return $"Chi tiêu tháng này giảm {Math.Abs(percentChange):0.##}% so với tháng trước. Tiếp tục duy trì nhé.";
    }
}
