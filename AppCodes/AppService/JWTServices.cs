using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Text.Json;
using mvcDapper3.Models;

namespace mvcDapper3.AppCodes.AppService
{
    public class JWTServices
    {
        private readonly IConfiguration _config;

        public JWTServices(IConfiguration config)
        {
            _config = config; // 從 appsetting.json 讀取設定
        }

        public string GenerateToken(Users user)
        {
            // Validate required fields to prevent ArgumentNullException
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User object cannot be null");
            
            if (string.IsNullOrEmpty(user.UserNo))
                throw new ArgumentException("UserNo cannot be null or empty", nameof(user.UserNo));
            
            if (string.IsNullOrEmpty(user.UserName))
                throw new ArgumentException("UserName cannot be null or empty", nameof(user.UserName));
            
            if (string.IsNullOrEmpty(user.RoleNo))
                throw new ArgumentException("RoleNo cannot be null or empty", nameof(user.RoleNo));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SignKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserNo),
                new Claim(ClaimTypes.Role, user.RoleNo),
                new Claim("Settings", user.Settings ?? "{}"),
                new Claim("CalendarPreference", user.CalendarPreference ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SignKey"]);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
