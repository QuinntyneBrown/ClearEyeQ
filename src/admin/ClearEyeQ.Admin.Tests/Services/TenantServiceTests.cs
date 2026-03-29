using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClearEyeQ.Admin.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.Admin.Tests.Services;

public sealed class TenantServiceTests
{
    private static TenantService CreateService(HttpClient httpClient)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("GatewayApi").Returns(httpClient);
        return new TenantService(factory);
    }

    [Fact]
    public async Task GetTenantsAsync_ReturnsTenantsFromApi()
    {
        // Arrange
        var tenants = new List<TenantDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Acme Corp", Status = "Active", UsersCount = 10 },
            new() { Id = Guid.NewGuid(), Name = "Beta Inc", Status = "Active", UsersCount = 5 }
        };

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tenants)
            });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetTenantsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Acme Corp");
        result[1].Name.Should().Be("Beta Inc");
    }

    [Fact]
    public async Task GetTenantsAsync_ReturnsEmptyListOnError()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetTenantsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTenantAsync_ReturnsTenantById()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new TenantDto { Id = tenantId, Name = "Test Tenant", Status = "Active" };

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tenant)
            });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetTenantAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Tenant");
    }

    [Fact]
    public async Task CreateTenantAsync_ReturnsTrueOnSuccess()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Created));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        var service = CreateService(httpClient);

        // Act
        var result = await service.CreateTenantAsync("New Tenant");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateTenantAsync_ReturnsFalseOnError()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.BadRequest));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        var service = CreateService(httpClient);

        // Act
        var result = await service.DeactivateTenantAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
