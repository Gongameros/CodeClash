using System.Security.Claims;
using CodeClash.Identity.Attributes;
using Shouldly;

namespace CodeClash.Identity.Tests.Tests.Attributes;

public sealed class FromUserIdAttributeTests
{
    [Fact]
    public void Constructor_SetsClaimTypeToNameIdentifier()
    {
        var attr = new FromUserIdAttribute();

        attr.ClaimType.ShouldBe(ClaimTypes.NameIdentifier);
    }

    [Fact]
    public void Constructor_SetsAlternateClaimTypeToSub()
    {
        var attr = new FromUserIdAttribute();

        attr.AlternateClaimType.ShouldBe("sub");
    }

    [Fact]
    public void Constructor_SetsRequiredToTrue()
    {
        var attr = new FromUserIdAttribute();

        attr.Required.ShouldBeTrue();
    }

    [Fact]
    public void InheritsFromClaimAttribute()
    {
        var attr = new FromUserIdAttribute();

        attr.ShouldBeAssignableTo<FromClaimAttribute>();
    }

    [Fact]
    public void Attribute_TargetsPropertyOnly()
    {
        var attrUsage = typeof(FromUserIdAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), true)
            .Cast<AttributeUsageAttribute>()
            .Single();

        attrUsage.ValidOn.ShouldBe(AttributeTargets.Property);
        attrUsage.AllowMultiple.ShouldBeFalse();
    }
}
