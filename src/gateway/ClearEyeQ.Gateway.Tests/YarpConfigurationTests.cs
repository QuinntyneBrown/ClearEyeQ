using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClearEyeQ.Gateway.Tests;

public sealed class YarpConfigurationTests
{
    private readonly IConfiguration _configuration;

    public YarpConfigurationTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("yarp.json", optional: false)
            .Build();
    }

    [Fact]
    public void YarpConfig_HasReverseProxySection()
    {
        var section = _configuration.GetSection("ReverseProxy");

        section.Exists().Should().BeTrue();
    }

    [Fact]
    public void YarpConfig_Has11Routes()
    {
        var routes = _configuration.GetSection("ReverseProxy:Routes").GetChildren().ToList();

        routes.Should().HaveCount(11);
    }

    [Fact]
    public void YarpConfig_Has11Clusters()
    {
        var clusters = _configuration.GetSection("ReverseProxy:Clusters").GetChildren().ToList();

        clusters.Should().HaveCount(11);
    }

    [Theory]
    [InlineData("identity-route", "/api/identity/{**catch-all}", "identity-cluster")]
    [InlineData("scans-route", "/api/scans/{**catch-all}", "scans-cluster")]
    [InlineData("monitoring-route", "/api/monitoring/{**catch-all}", "monitoring-cluster")]
    [InlineData("environmental-route", "/api/environmental/{**catch-all}", "environmental-cluster")]
    [InlineData("diagnostics-route", "/api/diagnostics/{**catch-all}", "diagnostics-cluster")]
    [InlineData("predictions-route", "/api/predictions/{**catch-all}", "predictions-cluster")]
    [InlineData("treatments-route", "/api/treatments/{**catch-all}", "treatments-cluster")]
    [InlineData("clinical-route", "/api/clinical/{**catch-all}", "clinical-cluster")]
    [InlineData("notifications-route", "/api/notifications/{**catch-all}", "notifications-cluster")]
    [InlineData("billing-route", "/api/billing/{**catch-all}", "billing-cluster")]
    [InlineData("fhir-route", "/api/fhir/{**catch-all}", "fhir-cluster")]
    public void YarpConfig_RouteHasCorrectPathAndCluster(string routeId, string expectedPath, string expectedCluster)
    {
        var routeSection = _configuration.GetSection($"ReverseProxy:Routes:{routeId}");

        routeSection.Exists().Should().BeTrue($"Route '{routeId}' should exist");

        var path = routeSection["Match:Path"];
        path.Should().Be(expectedPath);

        var clusterId = routeSection["ClusterId"];
        clusterId.Should().Be(expectedCluster);
    }

    [Theory]
    [InlineData("identity-cluster", "http://localhost:5101")]
    [InlineData("scans-cluster", "http://localhost:5102")]
    [InlineData("monitoring-cluster", "http://localhost:5103")]
    [InlineData("environmental-cluster", "http://localhost:5104")]
    [InlineData("diagnostics-cluster", "http://localhost:5105")]
    [InlineData("predictions-cluster", "http://localhost:5106")]
    [InlineData("treatments-cluster", "http://localhost:5107")]
    [InlineData("clinical-cluster", "http://localhost:5108")]
    [InlineData("notifications-cluster", "http://localhost:5109")]
    [InlineData("billing-cluster", "http://localhost:5110")]
    [InlineData("fhir-cluster", "http://localhost:5111")]
    public void YarpConfig_ClusterHasCorrectDestination(string clusterId, string expectedAddress)
    {
        var address = _configuration[$"ReverseProxy:Clusters:{clusterId}:Destinations:destination1:Address"];

        address.Should().Be(expectedAddress);
    }

    [Fact]
    public void YarpConfig_AllRoutesHaveRateLimiterPolicy()
    {
        var routes = _configuration.GetSection("ReverseProxy:Routes").GetChildren();

        foreach (var route in routes)
        {
            var policy = route["RateLimiterPolicy"];
            policy.Should().Be("fixed", $"Route '{route.Key}' should have rate limiter policy 'fixed'");
        }
    }

    [Fact]
    public void YarpConfig_AllRoutesHaveAuthorizationPolicy()
    {
        var routes = _configuration.GetSection("ReverseProxy:Routes").GetChildren();

        foreach (var route in routes)
        {
            var policy = route["AuthorizationPolicy"];
            policy.Should().Be("default", $"Route '{route.Key}' should have authorization policy 'default'");
        }
    }
}
