using System.Text.Json.Serialization;

namespace CMaaS.Backend.Models
{
    //Use Json converter to serialize the enum as a string in JSON responses.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Admin,
        User,
        Viewer,
        SuperAdmin
    }
}