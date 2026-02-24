namespace CodeClash.Results;

public enum ErrorType
{
    None = 0,
    Failure = 1,      // 500
    Validation = 2,   // 400
    NotFound = 3,     // 404
    Conflict = 4,     // 409
    Unauthorized = 5, // 401
    Forbidden = 6,    // 403
    Unavailable = 7   // 503
}
