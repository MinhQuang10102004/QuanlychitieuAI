using System.Collections.Generic;
using Microsoft.ML;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services
{
    public class ExpensePredictionService
    {
        private readonly MLContext _mlContext;

        public ExpensePredictionService()
        {
            _mlContext = new MLContext();
        }

        public ITransformer TrainModel(IEnumerable<ExpensePredictionModel> data)
        {
            var trainData = _mlContext.Data.LoadFromEnumerable(data);
            var pipeline = _mlContext.Regression.Trainers.Sdca(labelColumnName: nameof(ExpensePredictionModel.Expense), featureColumnName: nameof(ExpensePredictionModel.Month));
            return pipeline.Fit(trainData);
        }

        public float PredictNextMonthExpense(ITransformer model, float currentMonth)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ExpensePredictionModel, ExpensePredictionModel>(model);
            var prediction = predictionEngine.Predict(new ExpensePredictionModel { Month = currentMonth });
            return prediction.Expense;
        }
    }
}
