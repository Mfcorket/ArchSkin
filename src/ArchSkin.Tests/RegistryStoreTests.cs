using ArchSkin.Core.RegistryStore;
using Xunit;

namespace ArchSkin.Tests;

public class RegistryStoreTests
{
    [Fact]
    public void SetValue_And_GetValue_RoundTrip()
    {
        RegistryStore.SetValue("TestValue", "hello");
        var value = RegistryStore.GetValue<string>("TestValue");
        Assert.Equal("hello", value);
    }

    [Fact]
    public void GetValue_ReturnDefault_WhenKeyNotExists()
    {
        var value = RegistryStore.GetValue<string>("NonExistentKey12345", "default");
        Assert.Equal("default", value);
    }

    [Fact]
    public void ValueExists_ReturnFalse_WhenKeyNotExists()
    {
        var exists = RegistryStore.ValueExists("NonExistentKey12345");
        Assert.False(exists);
    }
}
