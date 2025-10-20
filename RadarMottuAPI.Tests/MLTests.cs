using FluentAssertions;
using RadarMottuAPI.ML;
using Xunit;

namespace RadarMottuAPI.Tests;

public class MLTests
{
    [Fact]
    public void EstimateDistance_Should_Be_Positive()
    {
        var svc = new MLEstimatorService();
        var meters = svc.PredictMeters(-60f);
        meters.Should().BeGreaterThan(0f);
    }
}
