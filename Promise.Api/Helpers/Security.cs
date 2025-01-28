using System.Text;
using System.Security.Cryptography;
using JWT.Algorithms;
using JWT;
using JWT.Exceptions;
using JWT.Serializers;
using Promise.Lib;

namespace Promise.Api;

public static class Security
{
    public const string AuthorizationHttpHeader = "Authorization";
    public const string PayLoadFieldLogin = "login";
    public const string PayLoadFieldAuth = "auth";
    public const string PayLoadFieldExp = "exp";

    private const int accessTokenLifetimeHours = 24;
    private const string bearerTokenPrefix = "Bearer ";
    private const int saltLengthLimit = 20; // 20 Bytes! (less than 32 characters after Base64 conversion)

    public static string GetHash(string input)
    {
        using (var crypto = SHA256.Create())
        {
            return GetCryptoHash(crypto, input);
        }
    }


    public static double GetAccessTokenLifetimeSeconds()
    {
        return UnixEpoch.GetSecondsSince(DateTime.Now.AddHours(accessTokenLifetimeHours));
    }

    [Obsolete]
    public static string CreateBearerJwt(IDictionary<string, object> payload, string secret)
    {
        IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        IJsonSerializer serializer = new JsonNetSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
        var token = encoder.Encode(payload, secret);
        return bearerTokenPrefix + token;
    }

    public static bool ValidateBearerJwtPrefix(string bearerJwt)
    {
        if (bearerJwt is null) return false;
        return bearerJwt.LastIndexOf(bearerTokenPrefix, StringComparison.Ordinal) == 0;
    }

    [Obsolete]
    public static IDictionary<string, object> GetBearerJwtLoad(string bearerJwt, string secret, bool verify = true)
    {
        var jwt = GetJwtFromBearerJwt(bearerJwt);
        var payload = GetJwtLoad(jwt, secret, verify);
        return payload;
    }

    [Obsolete]
    public static bool ValidateBearerAccessToken(string bearerJwt, string login, string secret)
    {
        if (!ValidateBearerJwtPrefix(bearerJwt)) return false;
        var jwt = GetJwtFromBearerJwt(bearerJwt);
        var payload = GetJwtLoad(jwt, secret, true);
        if (payload.Count == 0) return false;
        if (!payload.ContainsKey(PayLoadFieldLogin)) return false;
        if (!payload.ContainsKey(PayLoadFieldAuth)) return false;
        return login.Equals(payload[PayLoadFieldLogin].ToString(), StringComparison.OrdinalIgnoreCase)
               && payload[PayLoadFieldAuth].ToString() == true.ToString();
    }
    public static string GetPasswordHash(string password, string salt)
    {
        return GetHash(GetHash(password) + salt);
    }

    [Obsolete]
    private static IDictionary<string, object> GetJwtLoad(string jwt, string secret, bool verify)
    {
        IDictionary<string, object> payload = new Dictionary<string, object>();
        try
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            payload = decoder.DecodeToObject<IDictionary<string, object>>(jwt, secret, verify: verify);
        }
        catch (TokenExpiredException)
        {
            MainLogger.Log("Token has expired");
        }
        catch (SignatureVerificationException)
        {
            MainLogger.Log("Token has invalid signature");
        }
        catch (Exception e)
        {
            MainLogger.Log("JWT exception " + e);
        }
        return payload;
    }

    private static string GetJwtFromBearerJwt(string bearerJwt)
    {
        return bearerJwt[bearerTokenPrefix.Length..];
    }

    private static string GetCryptoHash(HashAlgorithm crypto, string input)
    {
        var bytes = crypto.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sBuilder = new StringBuilder();
        foreach (var b in bytes)
        {
            sBuilder.Append(b.ToString("x2"));
        }
        return sBuilder.ToString();
    }




    public static string GetSalt()
    {
        return GetSalt(saltLengthLimit);
    }
    private static string GetSalt(int maximumSaltLength)
    {
        var salt = new byte[maximumSaltLength];
#pragma warning disable SYSLIB0023 // Type or member is obsolete
        using (var random = new RNGCryptoServiceProvider())
        {
            random.GetNonZeroBytes(salt);
        }
#pragma warning restore SYSLIB0023 // Type or member is obsolete
        return Convert.ToBase64String(salt); ;
    }


    internal static string GetMaskedEmail(string email)
    {
        var emailParts = email.Split('@');
        if (emailParts.Length != 2) return "*not*valid*email";
        var localPart = emailParts[0];
        var domainPart = emailParts[1];
        var maskedLocalPart = localPart[..2] + "****" + localPart[^2..];
        var domainParts = domainPart.Split('.');
        if (domainParts.Length < 2) return "*not*valid*email";
        var maskedDomainPart = domainParts[0][..2] + "****" + domainParts[0][^2..] + "." + string.Join('.', domainParts.Skip(1));
        var maskedEmail = maskedLocalPart + "@" + maskedDomainPart;
        return maskedEmail;
    }

    internal static string GetMaskedTel(string tel)
    {
        if (string.IsNullOrEmpty(tel) || tel.Length < 4) return "*not*valid*tel";
        var maskedTel = new string('*', tel.Length - 4) + tel[^4..];
        return maskedTel;
    }

    internal static string GenerateTemporaryPassword()
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRS0123456789TUVWXYZ0123456789abcdefghijklmnopqrs0123456789tuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

}
