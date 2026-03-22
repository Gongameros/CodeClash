namespace CodeClash.Web.ApiClients.Models.Requests;

public record UpdateModuleRequest(
    string? Title,
    string? Description,
    int? Order,
    int? XpReward);
