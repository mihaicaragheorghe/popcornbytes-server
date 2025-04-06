namespace PopcornBytes.Contracts.Users;

public record ChangeUserPasswordRequest(string OldPassword, string NewPassword);
