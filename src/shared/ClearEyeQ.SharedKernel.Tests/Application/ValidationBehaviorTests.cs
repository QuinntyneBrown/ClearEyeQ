using ClearEyeQ.SharedKernel.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.SharedKernel.Tests.Application;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        var expectedResponse = new TestResponse("ok");
        next().Returns(expectedResponse);

        var result = await behavior.Handle(new TestRequest("test"), next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_ValidatorsPass_CallsNext()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(
            Arg.Any<ValidationContext<TestRequest>>(),
            Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([validator]);
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        var expectedResponse = new TestResponse("ok");
        next().Returns(expectedResponse);

        var result = await behavior.Handle(new TestRequest("test"), next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_ValidatorFails_ThrowsValidationException()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required")
        };

        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(
            Arg.Any<ValidationContext<TestRequest>>(),
            Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([validator]);
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

        var act = () => behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_MultipleValidators_AggregatesFailures()
    {
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1.ValidateAsync(
            Arg.Any<ValidationContext<TestRequest>>(),
            Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(
            [
                new ValidationFailure("Field1", "Error 1")
            ]));

        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2.ValidateAsync(
            Arg.Any<ValidationContext<TestRequest>>(),
            Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(
            [
                new ValidationFailure("Field2", "Error 2")
            ]));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([validator1, validator2]);
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

        var act = () => behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
    }

    #region Test Doubles

    public sealed record TestRequest(string Name) : IRequest<TestResponse>;
    public sealed record TestResponse(string Result);

    #endregion
}
