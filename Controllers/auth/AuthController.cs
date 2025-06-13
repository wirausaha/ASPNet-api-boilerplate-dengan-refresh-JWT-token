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

    public AuthController(IConfiguration config, 
        UserService userService , 
        SysTokenService tokenService,
        ValidasiTokenService validasitokenservice)
    {
        _config = config;
        _userservice = userService;
        _tokenservice = tokenService;
        _validasitokenservice = validasitokenservice;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Contoh validasi sederhana
        var userName = (request.username ?? "").Trim();
        var passWord = (request.password ?? "").Trim();

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
        {
            return Unauthorized(new { success = false, message = "Username atau password salah", token = "" });
        }

        // Cek apakah dan ambil data user dari database
        var userLogin = _userservice.GetUser(userName);
        if (string.IsNullOrEmpty(userLogin.UserId))
        {
            return Unauthorized(new { success = false, message = "Username atau password salah", token = "" });
        }

        // Cek apakah password valid 
        var userValid = _userservice.VerifyPassword(passWord, userLogin.Password!); // compare password
        if (userValid == false)
        {
            return Unauthorized(new { success = false, message = "Username atau password salah", token = "" });
        }

        // Modifikasi data userLogin jika perlu dari database
        string defaultCompanyCode = "0000";
        string companyRole = "Admin"; // default role, bisa diambil dari userLogin jika ada


        // buat token JWT
        var token = GenerateJwtToken(userName,  
            userLogin.Email + "",
            companyRole, 
            defaultCompanyCode, request.rememberme);

        // buat refresh token
        var refreshtoken = GenerateRefreshToken();

        // untuk pengetesan refreshtoken diberikan hanya beberapa menit disini menggunakan 30 menit atau 1 jam kalau remember me
        var expiresIn = request.rememberme ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(30);
        //var expiresIn = request.rememberme ? TimeSpan.FromDays(14) : TimeSpan.FromDays(7);

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
        return Ok(new { success = true, message = "Login berhasil", token = token, refreshtoken = refreshtoken });
        

    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        string bearerToken = HttpContext.Request.Headers["Authorization"]!;
        string token = bearerToken!.Replace("Bearer ", "").Trim();
        var tokenInfo = _tokenservice.GetTokenInfo(token);
        if (tokenInfo != null) 
        {
            _tokenservice.DisactivateToken(tokenInfo);
        }         
        HttpContext.SignOutAsync(); // 🔥 Hapus session user
        Console.WriteLine("Logout berhasil");
        return Ok(new { success = true, message = "Logout berhasil" });
    }

    [HttpPost("refreshtoken")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {

        // Pengecekan token saat refresh token tidak menggunakan ValidasiTokenServices
        // karena pasti hasilnya tidak valid

        request.accessToken = (request.accessToken ?? "").Trim();
        request.refreshToken = (request.refreshToken ?? "").Trim();

        // Gunakan token dari header Authorization bukan dari request body
        string bearerToken = HttpContext.Request.Headers["Authorization"]!;
        string token = bearerToken!.Replace("Bearer ", "").Trim();

        var userClaims = HttpContext.User;
        string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";

        if (userName == "" )
        {
            // Token invalid, tidak ada atau sudah expired. Coba nonaktifkan token di database
            // Gunakan token dari header Authorization
            var tokenInfo = _tokenservice.GetTokenInfo(token);
            if (tokenInfo != null) {
                if (tokenInfo.ExpireDate < DateTime.Now) {
                    _tokenservice.DisactivateToken(tokenInfo);
                    Console.WriteLine("access token and refresh token expired, please re login");
                    return BadRequest(new { success = false, message = "Access token and refresh token expired, please re-login" });
                }
                // Buat token baru dengan data dari tokenInfo
                userName = tokenInfo.UserId ?? "Demo1";
                string companyCode = tokenInfo.CompanyCode ?? "0000"; 
                string userRole = tokenInfo.UserRole ?? "Admin";
                string userEmail = tokenInfo.Email ?? "demo@demo.com";

                // Buat JWT token baru
                var newToken = GenerateJwtToken(userName,  userEmail, userRole, companyCode, request.rememberme);
                var newRefreshToken = GenerateRefreshToken();

                // nonaktifkan atau sebaiknya langsung dihapus token lama
                // untuk pengetesan, non-aktifkan saja token lama
                _tokenservice.DisactivateToken(tokenInfo);  

                // untuk pengetesan gunakan beberapa menit, disini menggunakan 30 menit atau 1 jam kalau remember me
                var expiresIn = request.rememberme ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(30);
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
                
                // Kembalikan token baru
                return Ok(new { success = true, token = newToken, refreshtoken = newRefreshToken });
            } else {
                // data token dari Bearer tidak ditemukan di claim dan di database
                return Unauthorized(new { success = false, message = "token tidak ditemukan di database, please re-login" });
            }
       } else {
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
        // untuk pengetesan gunakan hanya beberapa menit
        var expiresIn = rememberme ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(1);

        //var expiresIn = rememberme ? TimeSpan.FromDays(7) : TimeSpan.FromMinutes(15);
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

// Model untuk menangani data request login
public class LoginRequest
{
    public string username { get; set; } = "";
    public string password { get; set; } = ""; 
    public bool rememberme {get; set;} = false;
}