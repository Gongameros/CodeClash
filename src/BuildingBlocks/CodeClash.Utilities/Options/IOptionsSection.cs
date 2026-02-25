namespace CodeClash.Utilities.Options;

/// <summary>
/// Represents a contract for strongly typed options that define
/// their configuration section name.
/// </summary>
/// <remarks>
/// Implement this interface on an options class to eliminate the need
/// to pass configuration section names as strings when binding options.
/// </remarks>
public interface IOptionsSection
{
    /// <summary>
    /// Gets the configuration section name used to bind this options type.
    /// </summary>
    static abstract string SectionName { get; }
}
