namespace Pattrn.Routing.Tests;

public sealed class RouteConstraintValidationTests
{
    [Test]
    public async Task ValidateConstraintsAcceptsBuiltInScalarConstraints()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:int:min(1):max(99)}/{code:alpha:length(3)}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id:int:min(1):max(99)}/{code:alpha:length(3)}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/orders/42/abc")[0];

        var result = template.ValidateConstraints(match);

        await Assert.That(result.IsValid).IsTrue().Because("Expected all built-in route constraints to accept the captures.");
        await Assert.That(result.Failures.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ValidateConstraintsRejectsStructurallyMatchedButInvalidCapture()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:int:min(10)}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id:int:min(10)}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/orders/abc")[0];

        var result = template.ValidateConstraints(match);

        await Assert.That(result.IsValid).IsFalse().Because("Expected route-layer constraints to reject the structural match.");
        await Assert.That(result.Failures.Count).IsEqualTo(2);
        await Assert.That(result.Failures[0].Code).IsEqualTo("ROUTECONSTRAINT003");
        await Assert.That(result.Failures[0].ParameterName).IsEqualTo("id");
        await Assert.That(result.Failures[0].Constraint.Name).IsEqualTo("int");
        await Assert.That(result.Failures[0].Value).IsEqualTo("abc");
        await Assert.That(result.Failures[0].TemplateSegmentIndex).IsEqualTo(1);
        await Assert.That(result.Failures[0].PathSegmentIndex).IsEqualTo(1);
    }

    [Test]
    public async Task ValidateConstraintsSkipsOmittedOptionalParameter()
    {
        var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int?}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/archive/{year:int}/{month:int?}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/archive/2026")[0];

        var result = template.ValidateConstraints(match);

        await Assert.That(result.IsValid).IsTrue().Because("Expected omitted optional constrained segment to be accepted.");
    }

    [Test]
    public async Task ValidateConstraintsRejectsUnknownConstraintByDefault()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:tenant}");
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id:tenant}", "handler")
            .Build();

        var match = index.MatchRouteDetailedToArray("/orders/acme")[0];

        var result = template.ValidateConstraints(match);

        await Assert.That(result.IsValid).IsFalse().Because("Expected unknown constraints to fail closed by default.");
        await Assert.That(result.Failures.Count).IsEqualTo(1);
        await Assert.That(result.Failures[0].Code).IsEqualTo("ROUTECONSTRAINT002");
        await Assert.That(result.Failures[0].Constraint.Name).IsEqualTo("tenant");
    }

    [Test]
    public async Task ValidateConstraintsCanAllowUnknownConstraints()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:tenant}");
        var captures = new[] { new PatternCapture<string>("id", "acme", 1) };

        var result = template.ValidateConstraints(captures, new RouteConstraintValidationOptions { AllowUnknownConstraints = true });

        await Assert.That(result.IsValid).IsTrue().Because("Expected unknown constraints to be accepted when explicitly allowed.");
    }

    [Test]
    public async Task ValidateConstraintsSupportsCustomValidatorRegistry()
    {
        var template = RoutePattern.ParseTemplate("/tenants/{tenant:knownTenant}");
        var captures = new[] { new PatternCapture<string>("tenant", "acme", 1) };
        var registry = RouteConstraintValidatorRegistry.CreateDefault()
            .Add("knownTenant", static (value, constraint) => value == "acme");

        var result = template.ValidateConstraints(captures, new RouteConstraintValidationOptions { ValidatorRegistry = registry });

        await Assert.That(result.IsValid).IsTrue().Because("Expected custom tenant validator to accept acme.");
    }

    [Test]
    public async Task ValidateConstraintsSupportsRegexConstraint()
    {
        var template = RoutePattern.ParseTemplate("/products/{sku:regex(^[A-Z]+[0-9]+$)}");
        var good = new[] { new PatternCapture<string>("sku", "ABC12", 1) };
        var bad = new[] { new PatternCapture<string>("sku", "abc12", 1) };

        await Assert.That(template.ValidateConstraints(good).IsValid).IsTrue().Because("Expected regex route constraint to accept matching SKU.");
        await Assert.That(template.ValidateConstraints(bad).IsValid).IsFalse().Because("Expected regex route constraint to reject non-matching SKU.");
    }
}
