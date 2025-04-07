using PopcornBytes.Api.Users;

namespace PopcornBytes.UnitTests.TestUtils;

public static class UserTestUtils
{
    public static User CreateUser(
        Guid? id = null,
        string email = "michael@michaelscotpapercompany.com",
        string username = "little_kid_lover",
        string passwordHash = "Password123") =>
        new(id ?? Guid.NewGuid()) { Email = email, Username = username, PasswordHash = passwordHash };
}