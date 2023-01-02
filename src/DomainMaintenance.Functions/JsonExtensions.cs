using Newtonsoft.Json;

namespace DomainMaintenance.Functions;

public static class JsonExtensions
{
    public static string ToJson<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}