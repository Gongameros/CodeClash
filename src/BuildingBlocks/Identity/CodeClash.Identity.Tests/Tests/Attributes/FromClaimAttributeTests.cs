using CodeClash.Identity.Attributes;
using Shouldly;

namespace CodeClash.Identity.Tests.Tests.Attributes;

public sealed class FromClaimAttributeTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var attr = new FromClaimAttribute();

        attr.ClaimType.ShouldBeNull();
        attr.AlternateClaimType.ShouldBeNull();
        attr.Required.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithClaimType_SetsClaimType()
    {
        var attr = new FromClaimAttribute("email");

        attr.ClaimType.ShouldBe("email");
        attr.AlternateClaimType.ShouldBeNull();
        attr.Required.ShouldBeTrue();
    }

    [Fact]
    public void ClaimType_CanBeSet()
    {
        var attr = new FromClaimAttribute { ClaimType = "custom_claim" };

        attr.ClaimType.ShouldBe("custom_claim");
    }

    [Fact]
    public void AlternateClaimType_CanBeSet()
    {
        var attr = new FromClaimAttribute { AlternateClaimType = "alt_claim" };

        attr.AlternateClaimType.ShouldBe("alt_claim");
    }

    [Fact]
    public void Required_CanBeSetToFalse()
    {
        var attr = new FromClaimAttribute { Required = false };

        attr.Required.ShouldBeFalse();
    }

    [Fact]
    public void Attribute_TargetsPropertyOnly()
    {
        var attrUsage = typeof(FromClaimAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        attrUsage.ValidOn.ShouldBe(AttributeTargets.Property);
        attrUsage.AllowMultiple.ShouldBeFalse();
    }
}
