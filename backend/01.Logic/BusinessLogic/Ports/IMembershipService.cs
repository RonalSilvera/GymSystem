namespace BusinessLogic.Ports;

public interface IMembershipService
{
    Task<MembershipActivationResult> ActivateOrRenewAsync(int clientId, int membershipTypeId, DateTime? startDate, DateTime now);
}

public record MembershipActivationResult(int MembershipId, DateTime StartDate, DateTime EndDate, string Status);
