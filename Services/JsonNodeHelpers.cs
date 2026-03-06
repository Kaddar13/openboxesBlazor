using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenBoxesMobile.Blazor.Services;

public static class JsonNodeHelpers
{
    public static IReadOnlyList<JsonObject> ToObjectList(JsonNode? node)
    {
        if (node is null)
        {
            return [];
        }

        if (node is JsonArray arr)
        {
            return arr.OfType<JsonObject>().ToList();
        }

        if (node is JsonObject obj)
        {
            if (obj["data"] is JsonArray dataArr)
            {
                return dataArr.OfType<JsonObject>().ToList();
            }

            if (obj["items"] is JsonArray itemsArr)
            {
                return itemsArr.OfType<JsonObject>().ToList();
            }

            return [obj];
        }

        return [];
    }

    public static string Get(JsonObject obj, string path, string fallback = "-")
    {
        JsonNode? current = obj;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current is not JsonObject cobj || cobj[segment] is null)
            {
                return fallback;
            }

            current = cobj[segment];
        }

        return current switch
        {
            null => fallback,
            JsonValue value => value.TryGetValue<string>(out var str)
                ? str
                : value.ToJsonString(),
            _ => current.ToJsonString(new JsonSerializerOptions { WriteIndented = false })
        };
    }
}
