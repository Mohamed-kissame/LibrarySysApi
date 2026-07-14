using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibrarySys.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
        {
            string jwtKey = _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT key is missing.");

            string issuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");

            string audience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT audience is missing.");

            int expirationMinutes = int.Parse( _configuration["JwtSettings:ExpirationMinutes"] ?? "60");

            DateTime expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("fullName", user.FullName)
            };

            if (user.MemberID.HasValue)
            {
                claims.Add(new Claim("memberId", user.MemberID.Value.ToString()));
            }

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey) );

            SigningCredentials signingCredentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: signingCredentials
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenString, expiresAt);
        }
    }
}