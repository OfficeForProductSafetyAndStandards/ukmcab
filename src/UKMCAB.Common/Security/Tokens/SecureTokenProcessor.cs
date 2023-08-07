using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace UKMCAB.Common.Security.Tokens;

public interface ISecureTokenProcessor
{
    T? Disclose<T>(string token);
    string? Enclose<T>(T obj);
}

public class SecureTokenProcessor : ISecureTokenProcessor
{
    private readonly KeyIV _keyiv;

    public SecureTokenProcessor(string key)
    {
        Guard.IsNotNull(key.Clean(), $"{nameof(SecureTokenProcessor)}.ctor(key) parameter");
        _keyiv = KeyIV.Create(key);
    }

    public string? Enclose<T>(T obj)
    {
        var retVal = null as string;
        if (obj != null)
        {
            var text = obj is string ? (obj as string ?? throw new Exception("obj should not cast to null")) : (JsonSerializer.Serialize(obj) ?? throw new Exception("obj should not serialise to null"));
            var bytes = CryptoHelper.Encrypt(text, _keyiv); // encrypt
            retVal = Base64UrlEncoder.Encode(bytes); // url token encoding
        }
        return retVal;
    }

    public T? Disclose<T>(string token)
    {
        var retVal = default(T);
        if (!string.IsNullOrWhiteSpace(token))
        {
            try
            {
                var bytes = Base64UrlEncoder.DecodeBytes(token);
                var text = CryptoHelper.Decrypt(bytes, _keyiv);
                if (typeof(T) == typeof(string))
                {
                    return (T) (object) text;
                }
                else
                {
                    var obj = JsonSerializer.Deserialize<T>(text);
                    return obj;
                }
            }
            catch (Exception) { } // ignore
        }
        return retVal;
    }

}
