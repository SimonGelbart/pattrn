using static Pattrn.Routing.Tests.TestAssertions;

namespace Pattrn.Routing.Tests;

public sealed class RouteConstraintValidationTests
{
    [Test]
    public void ValidateConstraintsAcceptsBuiltInScalarConstraints()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:int:min(1):max(99)}/{code:alpha:length(3)}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id:int:min(1):max(99)}/{code:alpha:length(3)}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/orders/42/abc")[0];

        var result = template.ValidateConstraints(match);

        ShouldBeTrue(result.IsValid, "Expected all built-in route constraints to accept the captures.");
        ShouldEqual(result.Failures.Count, 0);
    }

    [Test]
    public void ValidateConstraintsRejectsStructurallyMatchedButInvalidCapture()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:int:min(10)}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id:int:min(10)}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/orders/abc")[0];

        var result = template.ValidateConstraints(match);

        ShouldBeFalse(result.IsValid, "Expected route-layer constraints to reject the structural match.");
        ShouldEqual(result.Failures.Count, 2);
        ShouldEqual(result.Failures[0].Code, "ROUTECONSTRAINT003");
        ShouldEqual(result.Failures[0].ParameterName, "id");
        ShouldEqual(result.Failures[0].Constraint.Name, "int");
        ShouldEqual(result.Failures[0].Value, "abc");
        ShouldEqual(result.Failures[0].TemplateSegmentIndex, 1);
        ShouldEqual(result.Failures[0].PathSegmentIndex, 1);
    }

    [Test]
    public void ValidateConstraintsSkipsOmittedOptionalParameter()
    {
        var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int?}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/archive/{year:int}/{month:int?}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/archive/2026")[0];

        var result = template.ValidateConstraints(match);

        ShouldBeTrue(result.IsValid, "Expected omitted optional constrained segment to be accepted.");
    }

    [Test]
    public void ValidateConstraintsRejectsUnknownConstraintByDefault()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:tenant}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id:tenant}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/orders/acme")[0];

        var result = template.ValidateConstraints(match);

        ShouldBeFalse(result.IsValid, "Expected unknown constraints to fail closed by default.");
        ShouldEqual(result.Failures.Count, 1);
        ShouldEqual(result.Failures[0].Code, "ROUTECONSTRAINT002");
        ShouldEqual(result.Failures[0].Constraint.Name, "tenant");
    }

    [Test]
    public void ValidateConstraintsCanAllowUnknownConstraints()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:tenant}");
        var captures = new[] { new PatternCapture<string>("id", "acme", 1) };

        var result = template.ValidateConstraints(captures, new RouteConstraintValidationOptions { AllowUnknownConstraints = true });

        ShouldBeTrue(result.IsValid, "Expected unknown constraints to be accepted when explicitly allowed.");
    }

    [Test]
    public void ValidateConstraintsSupportsCustomValidatorRegistry()
    {
        var template = RoutePattern.ParseTemplate("/tenants/{tenant:knownTenant}");
        var captures = new[] { new PatternCapture<string>("tenant", "acme", 1) };
        var registry = RouteConstraintValidatorRegistry.CreateDefault()
            .Add("knownTenant", static (value, constraint) => value == "acme");

        var result = template.ValidateConstraints(captures, new RouteConstraintValidationOptions { ValidatorRegistry = registry });

        ShouldBeTrue(result.IsValid, "Expected custom tenant validator to accept acme.");
    }

    [Test]
    public void ValidateConstraintsSupportsRegexConstraint()
    {
        var template = RoutePattern.ParseTemplate("/products/{sku:regex(^[A-Z]+[0-9]+$)}");
        var good = new[] { new PatternCapture<string>("sku", "ABC12", 1) };
        var bad = new[] { new PatternCapture<string>("sku", "abc12", 1) };

        ShouldBeTrue(template.ValidateConstraints(good).IsValid, "Expected regex route constraint to accept matching SKU.");
        ShouldBeFalse(template.ValidateConstraints(bad).IsValid, "Expected regex route constraint to reject non-matching SKU.");
    }
}
