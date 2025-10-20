using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace RadarMottuAPI.ML;

public class MLEstimatorService
{
    private readonly PredictionEngine<RssiInput, DistancePrediction> _engine;

    public MLEstimatorService()
    {
        var ml = new MLContext(seed: 42);
        var samples = new List<RssiInput>
        {
            new() { Rssi = -35, Distance = 1.0f },
            new() { Rssi = -45, Distance = 2.0f },
            new() { Rssi = -55, Distance = 4.0f },
            new() { Rssi = -60, Distance = 6.0f },
            new() { Rssi = -65, Distance = 9.0f },
            new() { Rssi = -70, Distance = 12.0f },
            new() { Rssi = -75, Distance = 18.0f },
            new() { Rssi = -80, Distance = 25.0f },
        };
        var data = ml.Data.LoadFromEnumerable(samples);

        var pipeline = ml.Transforms.CopyColumns("Label", nameof(RssiInput.Distance))
            .Append(ml.Transforms.Concatenate("Features", nameof(RssiInput.Rssi)))
            .Append(ml.Regression.Trainers.Sdca());

        var model = pipeline.Fit(data);
        _engine = ml.Model.CreatePredictionEngine<RssiInput, DistancePrediction>(model);
    }

    public float PredictMeters(float rssiDbm) =>
        _engine.Predict(new RssiInput { Rssi = rssiDbm }).Score;

    public class RssiInput { public float Rssi { get; set; } public float Distance { get; set; } }
    public class DistancePrediction { [ColumnName("Score")] public float Score { get; set; } }
}
