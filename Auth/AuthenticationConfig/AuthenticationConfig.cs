using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Auth.AuthenticationConfig
{
    public class Authentication
    {
        private readonly ILogger<Authentication> _logger;
        private readonly IWebHostEnvironment _host;

        public Authentication(
            IWebHostEnvironment host,
            ILogger<Authentication> logger)
        {
            _host = host;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GenerateJwtToken(string fullName, string userID)
        {
            string privateKey = File.ReadAllText(_host.WebRootPath + @"/content/RSA256/privateKey.pem");
            var claims = new List<Claim>();
            claims.Add(new Claim("userID", userID));
            claims.Add(new Claim("full-name", fullName));

            var token = CreateToken(claims, privateKey);
            return token;
        }

        /// <summary>
        /// Tạo access token để gửi cho ứng dụng khác
        /// </summary>
        /// <param name="claims">Thông tin cần có trong token</param>
        /// <param name="privateRsaKey">Khóa bí mật của thuật toán RSA sinh ra bằng openssl</param>
        /// <returns>Access token</returns>
        public string CreateToken(List<Claim> claims, string privateRsaKey)
        {
            try
            {
                RSAParameters rsaParams;
                using (var tr = new StringReader(privateRsaKey))
                {
                    var pemReader = new PemReader(tr);
                    var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                    if (keyPair == null)
                    {
                        throw new Exception("Could not read RSA private key");
                    }
                    var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                    rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParams);
                    Dictionary<string, object> payload = claims.ToDictionary(k => k.Type, v => (object)v.Value);
                    payload.Add("iss", "https://localhost:44311/api/users");
                    payload.Add("aud", "MinhXeOm");
                    payload.Add("exp", ((DateTimeOffset)DateTime.Now.AddMinutes(10)).ToUnixTimeSeconds());
                    payload.Add("nbf", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds());
                    return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("LocalHost exception in /AuthenticationConfig/CreateToken: " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Giải mã access token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string DecodeToken(string token)
        {

            try
            {
                string publicRsaKey = File.ReadAllText(_host.WebRootPath + @"/content/RSA256/publicKey.pem");
                RSAParameters rsaParams;

                using (var tr = new StringReader(publicRsaKey))
                {
                    var pemReader = new PemReader(tr);
                    var publicKeyParams = pemReader.ReadObject() as RsaKeyParameters;
                    if (publicKeyParams == null)
                    {
                        throw new Exception("Could not read RSA public key");
                    }
                    rsaParams = DotNetUtilities.ToRSAParameters(publicKeyParams);
                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParams);
                    // This will throw if the signature is invalid
                    return Jose.JWT.Decode(token, rsa, Jose.JwsAlgorithm.RS256);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("LocalHost exception in /AuthenticationConfig/DecodeToken: " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GenerateAccessToken(List<Claim> claims)
        {
            string privateKey = File.ReadAllText(_host.WebRootPath + @"/content/RSA256/privateKey.pem");
            var token = CreateAccessToken(claims, privateKey);
            return token;
        }

        /// <summary>
        /// Tạo access token để gửi cho ứng dụng khác
        /// </summary>
        /// <param name="claims">Thông tin cần có trong token</param>
        /// <param name="privateRsaKey">Khóa bí mật của thuật toán RSA sinh ra bằng openssl</param>
        /// <returns>Access token</returns>
        public string CreateAccessToken(List<Claim> claims, string privateRsaKey)
        {
            try
            {
                RSAParameters rsaParams;
                using (var tr = new StringReader(privateRsaKey))
                {
                    var pemReader = new PemReader(tr);
                    var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                    if (keyPair == null)
                    {
                        throw new Exception("Could not read RSA private key");
                    }
                    var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                    rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParams);
                    Dictionary<string, object> payload = claims.ToDictionary(k => k.Type, v => (object)v.Value);
                    payload.Add("iss", "https://google.com.vn");
                    payload.Add("aud", "https://google.com.vn");
                    payload.Add("exp", ((DateTimeOffset)DateTime.Now.AddDays(1)).ToUnixTimeSeconds());
                    payload.Add("nbf", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds());
                    return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("LocalHost exception in /AuthenticationConfig/CreateAccessToken: " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Giải mã access token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string DecodeAccessToken(string token)
        {

            try
            {
                string publicRsaKey = File.ReadAllText(_host.WebRootPath + @"/content/RSA256/publicKey.pem");
                RSAParameters rsaParams;

                using (var tr = new StringReader(publicRsaKey))
                {
                    var pemReader = new PemReader(tr);
                    var publicKeyParams = pemReader.ReadObject() as RsaKeyParameters;
                    if (publicKeyParams == null)
                    {
                        throw new Exception("Could not read RSA public key");
                    }
                    rsaParams = DotNetUtilities.ToRSAParameters(publicKeyParams);
                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParams);
                    // This will throw if the signature is invalid
                    return Jose.JWT.Decode(token, rsa, Jose.JwsAlgorithm.RS256);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("LocalHost exception in /AuthenticationConfig/DecodeAccessToken: " + ex.ToString());
                return null;
            }
        }
    }
}
