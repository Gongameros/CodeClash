namespace CodeClash.Web.ApiClients.Models.Requests;

public record AddModuleRequest(
    string Title,
    string? Description,
    int Order,
    int XpReward);
