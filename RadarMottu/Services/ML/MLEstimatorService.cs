// RadarMottuAPI/Services/ML/MLEstimatorService.cs
using Microsoft.ML;
using Microsoft.ML.Data;

namespace RadarMottuAPI.Services.ML;

public class MLEstimatorService
{
    private readonly MLContext _ml;
    private readonly ITransformer _model;
    private readonly object _lock = new();

    public MLEstimatorService()
    {
        _ml = new MLContext(seed: 42);

        var samples = new List<RssiInput>
        {
            new() { Rssi = -35, Distance = 1.0f  },
            new() { Rssi = -45, Distance = 2.0f  },
            new() { Rssi = -55, Distance = 4.0f  },
            new() { Rssi = -60, Distance = 6.0f  },
            new() { Rssi = -65, Distance = 9.0f  },
            new() { Rssi = -70, Distance = 12.0f },
            new() { Rssi = -75, Distance = 18.0f },
            new() { Rssi = -80, Distance = 25.0f },
        };

        var data = _ml.Data.LoadFromEnumerable(samples);

        var pipeline = _ml.Transforms.CopyColumns("Label", nameof(RssiInput.Distance))
            .Append(_ml.Transforms.Concatenate("Features", nameof(RssiInput.Rssi)))
            .Append(_ml.Transforms.NormalizeMinMax("Features"))
            .AppendCacheCheckpoint(_ml)
            .Append(_ml.Regression.Trainers.Sdca());

        _model = pipeline.Fit(data);
    }

    public float PredictMeters(float rssiDbm)
    {
        if (float.IsNaN(rssiDbm) || float.IsInfinity(rssiDbm))
            return float.NaN;

        var clampedRssi = Math.Clamp(rssiDbm, -100f, -30f);

        lock (_lock)
        {
            using var engine = _ml.Model.CreatePredictionEngine<RssiInput, DistancePrediction>(_model);
            var raw = engine.Predict(new RssiInput { Rssi = clampedRssi }).Score;

            var meters = Math.Clamp(raw, 0f, 100f);

            if (meters <= 0.01f && clampedRssi <= -85f)
            {
                const float txPowerAt1m = -59f;
                const float n = 2.0f;
                var fallback = Math.Pow(10, (txPowerAt1m - clampedRssi) / (10 * n));
                meters = (float)Math.Clamp(fallback, 0.1, 100.0);
            }

            return meters;
        }
    }

    public class RssiInput
    {
        public float Rssi { get; set; }
        public float Distance { get; set; }
    }

    public class DistancePrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}
