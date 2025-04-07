using PopcornBytes.Api.Users;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Users;

public class CreateUserValidatorTests
{
    [Fact]
    public void Validate_HappyPath()
    {
        // Arrange
        var user = UserTestUtils.CreateUser();

        // Act
        var result = new CreateUserValidator(user).Validate();

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("    ")]
    public void Validate_ShouldReturnError_WhenUsernameEmptyOrNull(string? username)
    {
        // Arrange
        var user = UserTestUtils.CreateUser(username: username!);

        // Act
        var result = new CreateUserValidator(user).Validate();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.EmptyUsername);
    }

    [Theory]
    [MemberData(nameof(UsernamesWithInvalidLength))]
    public void Validate_ShouldReturnError_WhenUsernameTooLongOrTooShort(string username)
    {
        // Arrange
        var user = UserTestUtils.CreateUser(username: username);

        // Act
        var result = new CreateUserValidator(user).Validate();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.InvalidUsernameLength);
    }

    [Theory]
    [MemberData(nameof(NotAllowedUsernameSymbols))]
    public void Validate_ShouldReturnError_WhenUsernameHasInvalidSymbols(char symbol)
    {
        // Arrange
        var user = UserTestUtils.CreateUser(username: "michaelscott" + symbol);

        // Act
        var result = new CreateUserValidator(user).Validate();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.BadUsernameFormat);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenUsernameHasAllowedSymbols()
    {
        // Arrange
        var user = UserTestUtils.CreateUser(username: "michael.scott_ppr-co");

        // Act
        var result = new CreateUserValidator(user).Validate();

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("foo")]
    [InlineData("foo@bar")]
    [InlineData("foo@bar.x")]
    [InlineData("foo@bar.long")]
    public void Validate_ShouldReturnError_WhenEmailHasBadFormat(string? email)
    {
        // Arrange
        var user = UserTestUtils.CreateUser(email: email!);

        // Act
        var result = new CreateUserValidator(user).Validate();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.BadEmailFormat);
    }

    public static TheoryData<string> UsernamesWithInvalidLength()
    {
        var data = new TheoryData<string>();
        for (int i = 1; i < User.UsernameMinLength; i++)
        {
            data.Add(new string('b', i));
        }

        data.Add(new string('b', User.UsernameMaxLength + 1));
        return data;
    }

    public static TheoryData<char> NotAllowedUsernameSymbols() =>
        new(@"!@#$%^&*()+=§±[]{}:;|\<,>/?`~🤠".ToCharArray());
}