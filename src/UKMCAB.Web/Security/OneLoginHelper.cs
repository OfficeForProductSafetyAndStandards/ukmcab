using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace UKMCAB.Web.Security;

public class OneLoginHelper
{
    public const string ClientAssertionTypeJwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

    public const string LoginCallbackPath = "/oidc";
    public const string LogoutCallbackPath = "/oidc-logout";

    public string ClientId { get; }
    public string Audience { get; }
    private string KeyPairPem { get; }

    public OneLoginHelper(IConfiguration configuration)
    {
        ClientId = configuration["OneLoginClientId"] ?? throw new Exception("GOV.UK One Login client id could not be found");
        var keyPairBase64 = configuration["OneLoginKeyPairBase64"] ?? throw new Exception("GOV.UK One Login key pair could not be found");
        var keyPairBytes = Convert.FromBase64String(keyPairBase64);
        KeyPairPem = Encoding.UTF8.GetString(keyPairBytes);
        Audience = configuration["OidcAudience"] ?? throw new Exception("OidcAudience is not configured");
    }

    public string CreateClientAuthJwt()
    {
        var rsaSecurityKey = GetRsaSecurityKey();
        var tokenHandler = new JwtSecurityTokenHandler { TokenLifetimeInMinutes = 5 };
        var securityToken = tokenHandler.CreateJwtSecurityToken(
            issuer: ClientId,
            expires: DateTime.Now.AddDays(1),
            audience: Audience,
            subject: new ClaimsIdentity(new List<Claim> { new Claim("sub", ClientId) }),
            signingCredentials: new SigningCredentials(rsaSecurityKey, "RS256")
        );
        var token = tokenHandler.WriteToken(securityToken);
        return token;
    }

    private RsaSecurityKey GetRsaSecurityKey()
    {
        StringReader stringReader = new(KeyPairPem);
        PemReader pemReader = new(stringReader);

        RsaPrivateCrtKeyParameters? privateKeyParams = null;
        object obj = pemReader.ReadObject();
        while (obj != null && obj is not RsaPrivateCrtKeyParameters)
        {
            obj = pemReader.ReadObject();
        }
        privateKeyParams = obj as RsaPrivateCrtKeyParameters;

        if (privateKeyParams != null)
        {
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters(privateKeyParams);
            RSACryptoServiceProvider rsa = new();
            rsa.ImportParameters(rsaParams);
            return new RsaSecurityKey(rsa);
        }
        else
        {
            throw new Exception("The private key was not found");
        }
    }
}
