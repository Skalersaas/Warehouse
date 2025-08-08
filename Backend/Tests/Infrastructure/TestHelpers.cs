using System.Text;
using System.Text.Json;

namespace Tests.Infrastructure;

public static class TestHelpers
{
    public static JsonSerializerOptions DefaultJsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    public static StringContent CreateJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, DefaultJsonOptions);
    }
}
