using PopcornBytes.Api.Kernel;

namespace PopcornBytes.UnitTests.Kernel;

public class LRUCacheTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsZeroOrNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new LRUCache<int, int>(-1, TimeSpan.FromMinutes(1)));        
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenCapacityIsPositive()
    {
        Assert.NotNull(new LRUCache<int, int>(1, TimeSpan.FromMinutes(1)));
    }
    
    [Fact]
    public void Set_ShouldCreateItem_WhenDoesNotExist()
    {
        // Arrange
        var cache = new LRUCache<string, string>(10, TimeSpan.FromMinutes(1));
        const string key = "key";
        const string value = "value";
        
        // Act
        cache.Set(key, value);
        
        // Assert
        Assert.True(cache.TryGetValue(key, out string v));
        Assert.Equal(value, v);
    }

    [Fact]
    public void Set_ShouldUpdateItem_WhenExists()
    {
        // Arrange
        var cache = new LRUCache<string, string>(10, TimeSpan.FromMinutes(1));
        const string key = "key";
        const string value = "value";
        const string newValue = "newValue";
        
        // Act
        cache.Set(key, value);
        cache.Set(key, newValue);
        
        // Assert
        Assert.True(cache.TryGetValue(key, out string v));
        Assert.Equal(newValue, v);
    }

    [Fact]
    public void Set_ShouldRemoveUnusedItem_WhenCapacityExceeded()
    {
        // Arrange
        var cache = new LRUCache<string, string>(1, TimeSpan.FromMinutes(1));
        const string key = "key";
        const string value = "value";
        const string newKey = "newKey";
        const string newValue = "newValue";
        
        // Act
        cache.Set(key, value);
        cache.Set(newKey, newValue);
        
        // Assert
        Assert.False(cache.TryGetValue(key, out string _));
        Assert.True(cache.TryGetValue(newKey, out string v));
        Assert.Equal(newValue, v);
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        var cache = new LRUCache<string, string>(10, TimeSpan.FromMinutes(1));
        const string key = "key";
        const string value = "value";
        cache.Set(key, value);
        
        // Act
        bool result = cache.TryGetValue(key, out string v);
        
        // Assert
        Assert.True(result);
        Assert.Equal(value, v);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Arrange
        var cache = new LRUCache<string, string>(10, TimeSpan.FromMinutes(1));
        
        // Act
        bool result = cache.TryGetValue("non_existing_key", out string v);
        
        // Assert
        Assert.False(result);
        Assert.Null(v);
    }

    [Fact]
    public async Task TryGetValue_ShouldReturnFalse_WhenItemExpired()
    {
        // Assert
        var cache = new LRUCache<string, string>(10, TimeSpan.FromMilliseconds(10));
        const string key = "key";
        cache.Set(key, "value");
        
        // Act
        await Task.Delay(11);
        bool result = cache.TryGetValue(key, out string v);
        
        // Assert
        Assert.False(result);
        Assert.Null(v);
    }
}