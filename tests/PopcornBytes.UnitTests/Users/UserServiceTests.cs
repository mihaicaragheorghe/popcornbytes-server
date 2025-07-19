using Microsoft.AspNetCore.Identity;

using Moq;

using PopcornBytes.Api.Security;
using PopcornBytes.Api.Users;
using PopcornBytes.Contracts.Users;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Users;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;

    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        _sut = new UserService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var expected = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(expected.Id))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetByIdAsync(expected.Id);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        // Act
        var user = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(user);
    }

    [Theory]
    [InlineData("Abc123!")] // Too short
    [InlineData("foobar123!")] // No upper case
    [InlineData("FOOBAR123!")] // No lower case
    [InlineData("FooBarBaz!")] // No numbers
    [InlineData("Hacking-Stoic-Commute-Molar-Nullify-Item-Shield123!")] // Too long
    public void IsStrongPassword_ShouldReturnFalse_WhenWeakPassword(string password)
    {
        Assert.False(UserService.IsStrongPassword(password));
    }

    [Theory]
    [InlineData("FooBar123")]
    [InlineData("Meditation-Sulk-Coffee1")]
    public void IsStrongPassword_ShouldReturnTrue_WhenStrongPassword(string password)
    {
        Assert.True(UserService.IsStrongPassword(password));
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnError_WhenWeakPassword()
    {
        // Arrange & Act
        var result = await _sut.CreateAsync("michaelscott", "mscott@dundermifflin.com", "1234");

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.WeakPassword);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnError_WhenEmailAlreadyExists()
    {
        // Arrange
        var user = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.CreateAsync(user.Username, user.Email, user.PasswordHash);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.EmailAlreadyExists);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnError_WhenUsernameAlreadyExists()
    {
        // Arrange
        var user = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.CreateAsync(user.Username, user.Email, user.PasswordHash);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.UsernameAlreadyExists);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnError_WhenValidationFails()
    {
        // Arrange
        var user = UserTestUtils.CreateUser(email: "bad_email");

        // Act
        var result = await _sut.CreateAsync(user.Username, user.Email, user.PasswordHash);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.BadEmailFormat);
    }

    [Fact]
    public async Task CreateAsync_ShouldHashPassword_BeforePersisting()
    {
        // Arrange
        var user = UserTestUtils.CreateUser();
        const string hashedPassword = "hashed_password";
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), user.PasswordHash))
            .Returns(hashedPassword);

        // Act
        _ = await _sut.CreateAsync(user.Username, user.Email, user.PasswordHash);

        // Assert
        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.PasswordHash == hashedPassword)), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenCreated()
    {
        // Arrange
        const string username = "michaelscott";
        const string email = "michaelscott@dundermifflin.com";
        const string password = "Password123!";
        const string hashedPassword = "hashed_password";
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), password))
            .Returns(hashedPassword);

        // Act
        var result = await _sut.CreateAsync(username, email, password);

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        Assert.NotEqual(result.Value, Guid.Empty);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);
        
        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), "foo", "bar");
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.UserNotFound);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        const string newUsername = "michael_scarn";
        const string newEmail = "mscarn@ttm.gov";
        var user = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        
        // Act
        var result = await _sut.UpdateAsync(user.Id, newUsername, newEmail);
        
        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user.Id, newUsername, newEmail), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_HappyPath()
    {
        // Arrange
        const string newUsername = "michael_scarn";
        const string newEmail = "mscarn@ttm.gov";
        var user = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        
        // Act
        var result = await _sut.UpdateAsync(user.Id, newUsername, newEmail);
        
        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        Assert.Equal(newUsername, user.Username);
        Assert.Equal(newEmail, user.Email);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnError_WhenWeakPassword()
    {
        // Arrange & Act
        var result = await _sut.ChangePasswordAsync(Guid.NewGuid(), "old_password", "1234");
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.WeakPassword);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange & Act
        var result = await _sut.ChangePasswordAsync(Guid.NewGuid(), "old_password", "FooBar123!");
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.UserNotFound);
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnError_WhenOldPasswordDoesNotMatch()
    {
        // Arrange
        const string oldPassword = "wrong_password";
        const string newPassword = "FooBar123!";
        var user = UserTestUtils.CreateUser();
        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, oldPassword))
            .Returns(PasswordVerificationResult.Failed);
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        
        // Act
        var result = await _sut.ChangePasswordAsync(user.Id, oldPassword, newPassword);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.WrongPassword);
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ShouldPersistChanges_WhenValidPassword()
    {
        // Arrange
        const string oldPassword = "old_password";
        const string newPassword = "NewPassword123!";
        const string newPasswordHash = "new_password_hash";
        var user = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, oldPassword))
            .Returns(PasswordVerificationResult.Success);
        _passwordHasherMock
            .Setup(x => x.HashPassword(user, newPassword))
            .Returns(newPasswordHash);
        
        // Act
        var result = await _sut.ChangePasswordAsync(user.Id, oldPassword, newPassword);
        
        // Assert
        Assert.Null(result.Error);
        _userRepositoryMock.Verify(x => x.UpdatePasswordHashAsync(user.Id, newPasswordHash), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);
        
        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid());
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.UserNotFound);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser_WhenUserFound()
    {
        // Arrange
        var user = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);
        
        // Act
        var result = await _sut.DeleteAsync(user.Id);
        
        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        _userRepositoryMock.Verify(m => m.DeleteAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock
            .Setup(m => m.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        
        // Assert
        var result = await _sut.LoginAsync(new LoginRequest("username", "password"));
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.UserNotFound);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenWrongPassword()
    {
        // Arrange
        const string wrongPassword = "wrong_password";
        var user = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, wrongPassword))
            .Returns(PasswordVerificationResult.Failed);
        
        // Act
        var result = await _sut.LoginAsync(new LoginRequest(user.Username, wrongPassword));
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(result.Error, UserErrors.WrongPassword);
    }

    [Fact]
    public async Task LoginAsync_ShouldGenerateJwtToken_WhenValidRequest()
    {
        // Arrange
        const string jwtToken = "jwt_token";
        const string plainPassword = "plain_password";
        var user  = UserTestUtils.CreateUser();
        _userRepositoryMock
            .Setup(m => m.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(m => m.VerifyHashedPassword(user, user.PasswordHash, plainPassword))
            .Returns(PasswordVerificationResult.Success);
        _tokenGeneratorMock
            .Setup(m => m.GenerateToken(user.Id, user.Username, user.Email))
            .Returns(jwtToken);
        
        // Act
        var result = await _sut.LoginAsync(new LoginRequest(user.Username, plainPassword));
        
        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        Assert.Equal(user.Id, result.Value?.Id);
        Assert.Equal(jwtToken, result.Value?.Token);
    }
}