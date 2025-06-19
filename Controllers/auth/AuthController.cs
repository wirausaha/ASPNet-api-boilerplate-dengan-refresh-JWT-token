/* ============================================================
 * AspApi - ASP.NET Core Web API Example
 *  Copyright (c) 2023 Kunta Soft by Fajrie R Aradea 
    *  Licensed under the MIT License
    * All rights reserved.

 Documentation is using google translate, I'm sorry if the translation is not accurate. 
============================================================= */  
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using AspApi.Models;
using AspApi.Services;
using AspApi.DTOServices;
using Microsoft.AspNetCore.Authentication;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{

    private readonly IConfiguration _config;
    private readonly UserService _userservice;
    private readonly SysTokenService _tokenservice;
    private readonly ValidasiTokenService _validasitokenservice;
    private readonly ILanguageProvider _lang;


    public AuthController(IConfiguration config, 
        UserService userService , 
        SysTokenService tokenService,
        ILanguageProvider lang,
        ValidasiTokenService validasitokenservice)
    {
        _config = config;
        _userservice = userService;
        _tokenservice = tokenService;
        _validasitokenservice = validasitokenservice;
        _lang = lang;
    }

    
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Validasi sederhana
        // Simple Validation
        var userName = (request.username ?? "").Trim();
        var passWord = (request.password ?? "").Trim();

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
        {
            return Unauthorized(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Username atau password salah" : 
                "Username or password failed", token = "" });
        }

        // Check is username exist in database
        // Cek apakah user valid dan ambil data user dari database
        var userLogin = _userservice.GetUser(userName);
        if (string.IsNullOrEmpty(userLogin.UserId))
        {
            return Unauthorized(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Username atau password salah" : "Username or password failed", token = "" });
        }

        // Check is password valid
        // Cek apakah password valid 
        var userValid = _userservice.VerifyPassword(passWord, userLogin.Password!); // compare password
        if (userValid == false)
        {
            return Unauthorized(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Username atau password salah" : "Username or password failed", token = "" });
        }

        // Modifikasi data userLogin jika perlu dari database
        string defaultCompanyCode = "0000"; // userLogin.CompanyCode ?? "0000" 
        string companyRole = "Admin"; // userLogin.UserRole ?? "0000" default role, bisa diambil dari userLogin jika ada

        // Generate JWT token
        // buat token JWT
        var token = GenerateJwtToken(userName,  
            userLogin.Email + "",
            companyRole, 
            defaultCompanyCode, request.rememberme);

        // Generate Refresh Token
        // buat refresh token
        var refreshtoken = GenerateRefreshToken();

        // for testing refreshtoken is given only a few minutes here using 30 minutes or 1 hour if remember me
        // untuk pengetesan refreshtoken diberikan hanya beberapa menit disini menggunakan 30 menit atau 1 jam kalau remember me
        
        //var expiresIn = request.rememberme ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(30);

        var expiresIn = request.rememberme ? TimeSpan.FromDays(14) : TimeSpan.FromDays(1);

        var sysToken = new SysToken
        {
            AccessToken = token,
            RefreshToken = refreshtoken,
            CompanyCode = defaultCompanyCode,
            UserId = userName,
            UserRole = companyRole,
            Email = userLogin.Email!,
            ExpireDate = DateTime.Now.Add(expiresIn),
        };
        _tokenservice.AddToken(sysToken);         

        // Console.WriteLine("Token : " + token);
        return Ok(new { success = true, message = (_lang.CurrentLang == "id") ? 
                    "Login berhasil" : "Login success", 
                    token = token, refreshtoken = refreshtoken });
        

    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {

        string bearerToken = HttpContext.Request.Headers["Authorization"]!;
        string token = bearerToken!.Replace("Bearer ", "").Trim();
        var tokenInfo = _tokenservice.GetTokenInfo(token);
        //Console.WriteLine("Logout token : " + token);
        if (tokenInfo != null) 
        {
            _tokenservice.DisactivateToken(tokenInfo);
        }         
        HttpContext.SignOutAsync(); // 🔥 Hapus session user
        Console.WriteLine("Logout berhasil");
        return Ok(new { success = true, message = 
            (_lang.CurrentLang == "id") ? "Logout berhasil" : "Logout success" });
    }

    [HttpPost("refreshtoken")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {

        // Token checking when refreshing token does not use ValidationTokenServices()
        // because the result is definitely invalid
        // Pengecekan token saat refresh token tidak menggunakan ValidasiTokenServices()
        // karena pasti hasilnya tidak valid
        request.accessToken = (request.accessToken ?? "").Trim();
        request.refreshToken = (request.refreshToken ?? "").Trim();

        // We use token from Authorization header not from request body
        // Gunakan token dari header Authorization bukan dari request body
        string bearerToken = HttpContext.Request.Headers["Authorization"]!;
        string token = bearerToken!.Replace("Bearer ", "").Trim();

        var userClaims = HttpContext.User;
        string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";

        if (userName == "" )
        {
            // Token is invalid, does not exist or has expired. Try disabling the token in the database.
            // Token invalid, tidak ada atau sudah expired. Coba nonaktifkan token di database
            var tokenInfo = _tokenservice.GetTokenInfo(token);
            if (tokenInfo != null) {
                // Catatan : Sebaiknya dibuatkan helper untuk membandingkan field timestamp dengan DateTime.Now
                if (tokenInfo.ExpireDate < DateTime.Now) {  
                    if (! _tokenservice.DisactivateToken(tokenInfo)) Console.WriteLine("Failed to disactivate token in database");
                    Console.WriteLine("access token and refresh token expired, please re login");
                    return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                "Access and refresh token kadaluarsa, silahkan login kembali" : 
                                "Access and refresh token expired, please re-login"  });
                }

                // Prepare a new token with data from tokenInfo
                // Siapkan token baru dengan data dari tokenInfo
                userName = tokenInfo.UserId ?? "Demo1";
                string companyCode = tokenInfo.CompanyCode ?? "0000"; 
                string userRole = tokenInfo.UserRole ?? "Admin";
                string userEmail = tokenInfo.Email ?? "demo@demo.com";

                // Create a new JWT token
                // Buat JWT token baru
                var newToken = GenerateJwtToken(userName,  userEmail, userRole, companyCode, request.rememberme);
                var newRefreshToken = GenerateRefreshToken();

                // Deactivate or preferably delete the old token immediately
                // nonaktifkan atau sebaiknya langsung dihapus token lama
                // untuk pengetesan, non-aktifkan saja token lama
                _tokenservice.DisactivateToken(tokenInfo);  

                // for testing use a few minutes, here use 30 minutes or 1 hour if remember me
                // untuk pengetesan gunakan beberapa menit, disini menggunakan 30 menit atau 1 jam kalau remember me

                //var expiresIn = request.rememberme ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(30);

                var expiresIn = request.rememberme ? TimeSpan.FromDays(14) : TimeSpan.FromDays(1);
                var newSysToken = new SysToken
                {
                    AccessToken = newToken,
                    RefreshToken = newRefreshToken,
                    CompanyCode = companyCode,
                    UserId = userName,
                    UserRole = userRole,
                    Email = userEmail,
                    ExpireDate = DateTime.Now.Add(expiresIn),
                };
                _tokenservice.AddToken(newSysToken);
                
                // return new token
                // Kembalikan token baru
                return Ok(new { success = true, token = newToken, refreshtoken = newRefreshToken });
            } else {
                // Token data from Bearer was not found in claim and in database
                // data token dari Bearer tidak ditemukan di claim dan di database
                return Unauthorized(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                "Token tidak ditemukan di database, please re-login" : 
                                "Token not found in database, please re-login" });
            }
       } else {
            // the token is still active, just return the data he sent
            // token masih aktif, kembalikan saja data yang dia kirim 
            return Ok(new { success = true, token = token, refreshtoken = request.refreshToken });
       }


    }

    public class RefreshTokenRequest {
        public string accessToken {get; set;} = "";
        public string refreshToken {get; set;} = "";
        public bool rememberme {get; set;} = false;
    }

    private string GenerateJwtToken(string username, string email, 
                    string userRole, string companyCode, bool rememberme)
    {
        //Console.WriteLine("Generate token untuk " + username);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, userRole),
            new Claim(ClaimTypes.NameIdentifier, companyCode),
            new Claim(ClaimTypes.Email, email)
        };

        // for testing use a few minutes, here use 1 minutes
        // untuk pengetesan gunakan beberapa menit, disini menggunakan1 menit
        
        //var expiresIn = rememberme ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(1);

        var expiresIn = rememberme ? TimeSpan.FromDays(1) : TimeSpan.FromHours(12);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiresIn),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)); // Generate random token
    }

}

// Model for handling login request data
// Model untuk menangani data request login
public class LoginRequest
{
    public string username { get; set; } = "";
    public string password { get; set; } = ""; 
    public bool rememberme {get; set;} = false;
}