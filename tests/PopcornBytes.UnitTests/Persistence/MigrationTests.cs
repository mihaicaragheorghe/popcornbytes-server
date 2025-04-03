using System.Data;

using Microsoft.Extensions.Logging;

using Moq;

using PopcornBytes.Api.Persistence;

namespace PopcornBytes.UnitTests.Persistence;

public class MigrationTests
{
    private readonly MigrationContext _migrationContext;

    public MigrationTests()
    {
        Mock<ILogger<MigrationsRunner>> mockLogger = new();
        Mock<IDbConnectionFactory> mockConnectionFactory = new();
        Mock<IDbConnection> mockDbConnection = new();

        mockConnectionFactory
            .Setup(cf => cf.CreateSqlConnection())
            .Returns(mockDbConnection.Object);

        _migrationContext =
            new MigrationContext(mockConnectionFactory.Object, mockLogger.Object) { UseTransaction = false };
    }

    private class TestMigration : Migration
    {
        public override long Version => 1;
        public override string Description => "Test";

        public bool UpWasCalled { get; private set; }
        public bool DownWasCalled { get; private set; }
        
        protected override void Up()
        {
            UpWasCalled = true;
        }

        protected override void Down()
        {
            DownWasCalled = true;
        }
    }

    [Fact, Trait("Category", "Integration")]
    public void Run_ShouldCallUp_WhenDirectionIsUp()
    {
        // Arrange
        var migration = new TestMigration();
        _migrationContext.Direction = MigrationDirection.Up;
        
        // Act
        migration.Run(_migrationContext);
        
        // Assert
        Assert.True(migration.UpWasCalled);
        Assert.False(migration.DownWasCalled);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void Run_ShouldCallDown_WhenDirectionIsDown()
    {
        // Arrange
        var migration = new TestMigration();
        _migrationContext.Direction = MigrationDirection.Down;
        
        // Act
        migration.Run(_migrationContext);
        
        // Assert
        Assert.False(migration.UpWasCalled);
        Assert.True(migration.DownWasCalled);
    }
}