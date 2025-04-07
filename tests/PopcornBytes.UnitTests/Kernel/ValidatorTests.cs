using PopcornBytes.Api.Kernel;

namespace PopcornBytes.UnitTests.Kernel;

public class ValidatorTests
{
    private record Show(string Title, int Seasons);

    private class ShowValidator : Validator<Show>
    {
        public ShowValidator(Show subject) : base(subject)
        {
            AddRule(show => !string.IsNullOrEmpty(show.Title), Error.Validation(code: "rule1"));
            
            AddRule(show => show.Seasons > 0, Error.Validation(code: "rule2"));
        }
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRulesMet()
    {
        // Arrange
        var show = new Show("Better Call Saul", 6);
        
        // Act
        var result = new ShowValidator(show).Validate();
        
        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void Validate_ShouldReturnError_WhenRuleFails()
    {
        // Arrange
        var show = new Show("", 6);
        
        // Act
        var result = new ShowValidator(show).Validate();
        
        // Assert
        Assert.True(result.IsError);
        Assert.NotNull(result.Error);
    }
}
