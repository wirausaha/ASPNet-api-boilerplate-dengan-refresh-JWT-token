/* ============================================================
 * AspApi - ASP.NET Core Web API Example
 *  Copyright (c) 2023 Kunta Soft by Fajrie R Aradea 
    *  Licensed under the MIT License
    * All rights reserved.
    
 Documentation is using google translate, I'm sorry if the translation is not accurate. 
============================================================= */    
using Microsoft.AspNetCore.Mvc;
using AspApi.Models;
using AspApi.Utilities;
using AspApi.Services;
using System.Text.RegularExpressions;
using AspApi.Helpers;

[Route("api/auth")]
[ApiController]
public class RegisterController : ControllerBase
{

    private readonly IConfiguration _config;
    private readonly UserService _userservice;

    private readonly ILanguageProvider _lang;

    public RegisterController(IConfiguration config,
                ILanguageProvider lang,  
                UserService userService)
    {
        _config = config;
        _userservice = userService;
        _lang = lang;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {

        /* ============================
        | Simple Validation
        ============================= */ 
        Console.WriteLine("Username : " + request.username);
        Console.WriteLine("Email : " + request.email);
        Console.WriteLine("Password : " + request.password);
        
        Console.WriteLine(DateTime.Now.ToString() + " : Enter api/auth/register");

        var userName = (request.username ?? "").Trim();
        var email = (request.email ?? "").Trim();
        var passWord = (request.password ?? "").Trim();

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passWord))
        {
            
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Username, email, atau password tidak boleh kosong" : 
                "Username, email, or password could not be empty" }    );
        } 

        string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$"; // Pola regex untuk email
        if (! Regex.IsMatch(email, pattern)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Format email salah" : "Wrong email format" });
        };

        if (userName.Length < 6 || userName.Length > 20 || passWord.Length < 6 || passWord.Length > 20) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                        "Username dan password minimal harus berisi 6 - 20 karakter" : 
                        "Username and password must contain at least 6 - 20 characters" }    );
        }

        if (email.Length < 11 || email.Length > 50) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Email harus berisi 11 - 50 karakter" : "Email must contain 11 - 50 characters" }    );
        }

        pattern = @"^[a-zA-Z0-9]+$";
        if (! Regex.IsMatch(userName, pattern)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                "Username hanya boleh berisi huruf dan angka" : 
                "Usernames can only contain letters and numbers" }    );
        }        

        var isValid = !_userservice.UsernameOrEmailExist(userName, email);
        if (! isValid) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                        "Username atau email sudah ada di database" : 
                        "Username or email already exists in the database" });
        }
        // If validation passes, proceed to create a new user
        // Jika semua validasi berhasil, lanjutkan untuk membuat user baru
        User UpdateUser = new User();
        Random random = new Random();
        int randomNumber = random.Next(1, 10); // dapatkan angka acak antara 1 dan 10
        var TokenUserName = userName;        
        UpdateUser.IsActive = 1;
        UpdateUser.UserId = RandomStringGenerator.GenerateRandomString(20);
        UpdateUser.UserName = userName;
        UpdateUser.Email = email;
        UpdateUser.Password = passWord;
        var publicUrl = Path.Combine("wwwroot", "images", "avatars", randomNumber.ToString() + ".jpg");
        UpdateUser.Avatar200x200 = publicUrl;
        UpdateUser.TermsAgrement = request.termsagrement ? 1 : 0; 
        _userservice.AddUser(UpdateUser);
        return Ok(new { success = true, message = (_lang.CurrentLang == "id") ? 
                    "register berhasil, silahkan login" : "" });
    }

}

// Model to handle registration request data
// Model untuk menangani data request login
public class RegisterRequest
{
    public string username { get; set; } = "";
    public string email { get; set; } = "";
    public string password { get; set; } = ""; 
    public bool termsagrement { get; set; } = false;
}