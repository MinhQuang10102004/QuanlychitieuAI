using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services;

public class CalculateBudget
{
    private readonly ExpensePredictionService _predictionService;

    public CalculateBudget(ExpensePredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    public decimal SuggestBudget(IEnumerable<ExpensePredictionModel> history)
    {
        if (history == null || !history.Any())
        {
            return 0m;
        }

        var model = _predictionService.TrainModel(history);
        var nextMonth = GetNextMonth(history);
        var predictedExpense = _predictionService.PredictNextMonthExpense(model, nextMonth);
        return Math.Round((decimal)predictedExpense * 1.1m, 2); // thêm 10% dự phòng
    }

    private static float GetNextMonth(IEnumerable<ExpensePredictionModel> history)
    {
        var maxMonth = history.Max(h => h.Month);
        return maxMonth + 1f;
    }
}
