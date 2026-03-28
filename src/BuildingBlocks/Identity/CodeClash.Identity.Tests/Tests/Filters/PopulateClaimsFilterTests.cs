using System.Security.Claims;
using CodeClash.Identity.Attributes;
using CodeClash.Identity.Filters;
using CodeClash.Identity.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace CodeClash.Identity.Tests.Tests.Filters;

public sealed class PopulateClaimsFilterTests
{
    private readonly PopulateClaimsFilter _sut = new();

    // --- Test model classes ---

    private class SimpleCommand
    {
        [FromUserId]
        public string UserId { get; set; } = default!;
    }

    private class CommandWithExplicitClaim
    {
        [FromClaim("preferred_username")]
        public string Username { get; set; } = default!;
    }

    private class CommandWithAlternateClaim
    {
        [FromClaim(ClaimType = "primary_id", AlternateClaimType = "sub")]
        public string Id { get; set; } = default!;
    }

    private class CommandWithOptionalClaim
    {
        [FromClaim("nickname", Required = false)]
        public string? Nickname { get; set; }
    }

    private class CommandWithGuid
    {
        [FromUserId]
        public Guid UserId { get; set; }
    }

    private class CommandWithInt
    {
        [FromClaim("age")]
        public int Age { get; set; }
    }

    private class CommandWithLong
    {
        [FromClaim("timestamp")]
        public long Timestamp { get; set; }
    }

    private class CommandWithBool
    {
        [FromClaim("is_active")]
        public bool IsActive { get; set; }
    }

    private class CommandWithReadOnly
    {
        [FromClaim("val")]
        public string ReadOnly { get; } = "original";
    }

    private class CommandWithNoAttributes
    {
        public string Name { get; set; } = "original";
    }

    // --- Helpers ---

    private static ActionExecutingContext CreateContext(
        ClaimsPrincipal user,
        Dictionary<string, object?> arguments)
    {
        var httpContext = new DefaultHttpContext { User = user };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            arguments!,
            controller: null!);
    }

    // --- Unauthenticated ---

    [Fact]
    public void OnActionExecuting_UnauthenticatedUser_SkipsPopulation()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateUnauthenticated();
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.UserId.ShouldBe(default!);
    }

    // --- Null arguments ---

    [Fact]
    public void OnActionExecuting_NullArgument_SkipsWithoutError()
    {
        var user = ClaimsPrincipalFactory.CreateWithUserId("user-1");
        var context = CreateContext(user, new() { ["command"] = null });

        Should.NotThrow(() => _sut.OnActionExecuting(context));
    }

    // --- Basic population ---

    [Fact]
    public void OnActionExecuting_WithUserId_PopulatesFromNameIdentifier()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateWithUserId("user-42");
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.UserId.ShouldBe("user-42");
    }

    [Fact]
    public void OnActionExecuting_WithSubClaim_PopulatesViaAlternate()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateWithSubClaim("sub-99");
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.UserId.ShouldBe("sub-99");
    }

    [Fact]
    public void OnActionExecuting_ExplicitClaimType_PopulatesCorrectly()
    {
        var command = new CommandWithExplicitClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("preferred_username", "johndoe"));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.Username.ShouldBe("johndoe");
    }

    // --- Alternate claim ---

    [Fact]
    public void OnActionExecuting_PrimaryMissing_UsesAlternateClaim()
    {
        var command = new CommandWithAlternateClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("sub", "alt-id"));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.Id.ShouldBe("alt-id");
    }

    // --- Optional claims ---

    [Fact]
    public void OnActionExecuting_OptionalClaimMissing_DoesNotThrow()
    {
        var command = new CommandWithOptionalClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated();
        var context = CreateContext(user, new() { ["command"] = command });

        Should.NotThrow(() => _sut.OnActionExecuting(context));
        command.Nickname.ShouldBeNull();
    }

    [Fact]
    public void OnActionExecuting_OptionalClaimPresent_Populates()
    {
        var command = new CommandWithOptionalClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("nickname", "Johnny"));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.Nickname.ShouldBe("Johnny");
    }

    // --- Required claim missing ---

    [Fact]
    public void OnActionExecuting_RequiredClaimMissing_ThrowsInvalidOperation()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(); // no claims
        var context = CreateContext(user, new() { ["command"] = command });

        var ex = Should.Throw<InvalidOperationException>(
            () => _sut.OnActionExecuting(context));

        ex.Message.ShouldContain("Required claim");
        ex.Message.ShouldContain("UserId");
    }

    // --- Type conversions ---

    [Fact]
    public void OnActionExecuting_GuidProperty_ConvertsCorrectly()
    {
        var command = new CommandWithGuid();
        var guid = Guid.NewGuid();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.NameIdentifier, guid.ToString()));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.UserId.ShouldBe(guid);
    }

    [Fact]
    public void OnActionExecuting_IntProperty_ConvertsCorrectly()
    {
        var command = new CommandWithInt();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("age", "30"));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.Age.ShouldBe(30);
    }

    [Fact]
    public void OnActionExecuting_LongProperty_ConvertsCorrectly()
    {
        var command = new CommandWithLong();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("timestamp", "9876543210"));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.Timestamp.ShouldBe(9876543210L);
    }

    [Fact]
    public void OnActionExecuting_BoolProperty_ConvertsCorrectly()
    {
        var command = new CommandWithBool();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("is_active", "true"));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.IsActive.ShouldBeTrue();
    }

    // --- Read-only property ---

    [Fact]
    public void OnActionExecuting_ReadOnlyProperty_DoesNotThrow()
    {
        var command = new CommandWithReadOnly();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("val", "new"));
        var context = CreateContext(user, new() { ["command"] = command });

        Should.NotThrow(() => _sut.OnActionExecuting(context));
        command.ReadOnly.ShouldBe("original");
    }

    // --- No attributes ---

    [Fact]
    public void OnActionExecuting_NoAttributes_DoesNotModifyObject()
    {
        var command = new CommandWithNoAttributes();
        var user = ClaimsPrincipalFactory.CreateFullUser();
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.Name.ShouldBe("original");
    }

    // --- Multiple arguments ---

    [Fact]
    public void OnActionExecuting_MultipleArguments_PopulatesAll()
    {
        var cmd1 = new SimpleCommand();
        var cmd2 = new CommandWithExplicitClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.NameIdentifier, "uid-1"),
            new Claim("preferred_username", "testuser"));
        var context = CreateContext(user, new()
        {
            ["cmd1"] = cmd1,
            ["cmd2"] = cmd2
        });

        _sut.OnActionExecuting(context);

        cmd1.UserId.ShouldBe("uid-1");
        cmd2.Username.ShouldBe("testuser");
    }

    // --- OnActionExecuted ---

    [Fact]
    public void OnActionExecuted_DoesNothing()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            controller: null!);

        // Should not throw
        Should.NotThrow(() => _sut.OnActionExecuted(context));
    }

    // --- Empty string claim ---

    [Fact]
    public void OnActionExecuting_EmptyStringClaim_SetsNull()
    {
        var command = new CommandWithOptionalClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("nickname", ""));
        var context = CreateContext(user, new() { ["command"] = command });

        _sut.OnActionExecuting(context);

        command.Nickname.ShouldBeNull();
    }
}
