using Microsoft.Extensions.Configuration;

using PopcornBytes.Api.Persistence;

namespace PopcornBytes.UnitTests.Persistence;

public class DbConnectionFactoryTests
{
    private const string ConfigPath = "ConnectionStrings:PopcornDB";

    [Fact, Trait("Category", "Integration")]
    public void CreateSqlConnection_ShouldThrowArgumentException_WhenConnectionStringIsNull()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()!)
            .Build();

        var sut = new DbConnectionFactory(configuration);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sut.CreateSqlConnection());
    }

    [Fact, Trait("Category", "Integration")]
    public void CreateSqlConnection_ShouldThrowArgumentException_WhenConnectionStringIsEmpty()
    {
        // Arrange
        var settings = new Dictionary<string, string>() { { ConfigPath, string.Empty } };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();

        var sut = new DbConnectionFactory(configuration);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.CreateSqlConnection());
    }

    [Fact, Trait("Category", "Integration")]
    public void CreateSqlConnection_ShouldCreateConnection_WhenConfigurationIsValid()
    {
        // Arrange
        const string expected = "Data Source = ::memory::";
        var settings = new Dictionary<string, string>() { { ConfigPath, expected } };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();

        var sut = new DbConnectionFactory(configuration);

        // Act
        var actual = sut.CreateSqlConnection();

        // Assert
        Assert.Equal(expected, actual.ConnectionString);
    }
}