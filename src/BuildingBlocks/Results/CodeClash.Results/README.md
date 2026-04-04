# CodeClash.Result

A lightweight, opinionated Result pattern library for ASP.NET Core minimal APIs, designed to be shared across microservices.

## Quick Start

### 1. Define domain errors as static constants

```csharp
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");

    public static Error DuplicateEmail(string email) =>
        Error.Conflict("User.DuplicateEmail", $"A user with email '{email}' already exists.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("User.InvalidCredentials", "The provided credentials are invalid.");

    public static readonly Error EmailRequired =
        Error.Validation("User.EmailRequired", "Email is required.");

    public static readonly Error PasswordTooShort =
        Error.Validation("User.PasswordTooShort", "Password must be at least 8 characters.");
}
```

### 2. Return Results from your services/handlers

```csharp
public class UserService(AppDbContext db)
{
    public async Task<Result<UserResponse>> GetByIdAsync(Guid id)
    {
        var user = await db.Users.FindAsync(id);

        if (user is null)
            return Result.Failure<UserResponse>(UserErrors.NotFound(id));

        return new UserResponse(user.Id, user.Name, user.Email);
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        // Collect validation errors
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(UserErrors.EmailRequired);

        if (request.Password.Length < 8)
            errors.Add(UserErrors.PasswordTooShort);

        if (errors.Count > 0)
            return Result.ValidationFailure<UserResponse>([.. errors]);

        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            return Result.Failure<UserResponse>(UserErrors.DuplicateEmail(request.Email));

        var user = new User(request.Name, request.Email, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
```

### 3. Map to HTTP responses in your endpoints

```csharp
app.MapGet("/users/{id:guid}", async (Guid id, UserService svc) =>
    (await svc.GetByIdAsync(id)).ToProblemDetails());

app.MapPost("/users", async (CreateUserRequest req, UserService svc) =>
    (await svc.CreateAsync(req)).ToCreatedProblemDetails($"/users/{{id}}"));

app.MapDelete("/users/{id:guid}", async (Guid id, UserService svc) =>
    (await svc.DeleteAsync(id)).ToNoContentProblemDetails());
```

### 4. Resulting ProblemDetails responses

**404 Not Found:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "User with id '550e8400-e29b-41d4-a716-446655440000' was not found.",
  "code": "User.NotFound"
}
```

**400 Bad Request (validation):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": [
    { "code": "User.EmailRequired", "description": "Email is required." },
    { "code": "User.PasswordTooShort", "description": "Password must be at least 8 characters." }
  ]
}
```

**409 Conflict:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "A user with email 'john@example.com' already exists.",
  "code": "User.DuplicateEmail"
}
```

## Railway-oriented chaining

```csharp
public async Task<Result<OrderConfirmation>> PlaceOrderAsync(PlaceOrderRequest request) =>
    (await ValidateOrder(request))
        .Ensure(order => order.Items.Count > 0, OrderErrors.EmptyOrder)
        .Bind(order => CalculateTotal(order))
        .Tap(order => logger.LogInformation("Order placed: {Total}", order.Total))
        .Map(order => new OrderConfirmation(order.Id, order.Total));
```

## Error Type → HTTP Status Mapping

| ErrorType     | HTTP Status |
|---------------|-------------|
| Validation    | 400         |
| Unauthorized  | 401         |
| Forbidden     | 403         |
| NotFound      | 404         |
| Conflict      | 409         |
| Failure       | 500         |
| Unavailable   | 503         |
