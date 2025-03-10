﻿using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace UKMCAB.Common.Security;
public class JsonBase64UrlToken
{
    public static string Serialize<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        return Base64UrlEncoder.Encode(json);
    }

    public static T? Deserialize<T>(string token)
    {
        if (token.IsNotNullOrEmpty())
        {
            var json = Base64UrlEncoder.Decode(token);
            return JsonSerializer.Deserialize<T>(json);
        }
        else
        {
            return default;
        }
    }

    public static string Pipe<T>(string token, Action<T> pipe)
    {
        var obj = Deserialize<T>(token);
        pipe(obj);
        return Serialize(obj);
    }
}
