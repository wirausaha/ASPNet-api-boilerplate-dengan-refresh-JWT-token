using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using AspApi.Models;
using AspApi.Services;
using AspApi.Helpers;
using AspApi.DTOServices;
using AspApi.Utilities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[Route("api/user")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILanguageProvider _lang;
    private readonly ValidasiTokenService _validasiTokenService;
    
    private readonly IWebHostEnvironment _env;

    public UserController(UserService userService, 
                ValidasiTokenService validasiTokenService, 
                IWebHostEnvironment env,
                ILanguageProvider lang)
    {
        _userService = userService;
        _validasiTokenService = validasiTokenService;
        _lang = lang;
        _env = env;
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count()
    {
        int count = await _userService.CountAsync();
        return Ok(new { success = true, count });
    }

    [HttpGet("getuserswithpagination")]
    public IActionResult GetUserWithPagination(int draw = 0, int page = 1, 
            int limit = 10, string? filter = null) {

        Console.WriteLine("Filter :", filter);
        page = page < 1 ? 0 : page;
        limit = limit < 1 ? 1 : limit;

        var userforDataTable = _userService.GetUserWithPagination(draw, (page-1) * limit, limit, filter);                    
        return Ok(new { success = true, userlist = userforDataTable });
    }


    [HttpGet("userexists")]
    public IActionResult UserExists(string userName)
    {
        Console.WriteLine("UserExists: " + userName);

        userName = (userName ?? "").Trim();
        if (string.IsNullOrEmpty(userName)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Username tidak boleh kosong" : "Username could not be empty" });
        }
        var userExists = _userService.UserExist(userName);
        return Ok(new { success = true, exists = userExists });
    }

    /*=================================
    | Mengambil data profil dengan format class UserDataDtos
    | Format ini sama dengan setiap baris yang di output ke datatable
    ==================================*/
    [HttpGet("getmyprofile")]
    public IActionResult GetMyProfile()
    {
        var userClaims = HttpContext.User;
        string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";
        if (string.IsNullOrEmpty(userName)) {
            return Unauthorized(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Anda tidak berhak" : "Access not allowed" });
        }

        userName = (userName ?? "").Trim();
        var userData = _userService.GetUserDataDtos(userName);
        return Ok(new { success = true, user = userData });
    }

    [HttpGet("emailexists")]
    public IActionResult EmailExists(string email)
    {
        var userClaims = HttpContext.User;        

       /*  if (! _validasiTokenService.TokenValid(userClaims))
        {
            Console.WriteLine(DateTime.Now.ToString() + " api/user/emailexists() Unauthorized");
            return Unauthorized(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Anda tidak berhak" : "Unauthorized" });
        } */

        email = (email ?? "").Trim();
        if (string.IsNullOrEmpty(email)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Email tidak boleh kosong" : "Email could not be empty" });
        }
        var emailExists = _userService.EmailExist(email);
        return Ok(new { success = true, exists = emailExists });
    }

    [HttpGet("getmyrole")]
    public IActionResult GetMyRole()
    {
        var userClaims = HttpContext.User;
        string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";
        if (string.IsNullOrEmpty(userName)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Username tidak boleh kosong" : "Username could not be empty" });
        }
        var myRole = _userService.GetMyRole(userName!);
        return Ok(new { success = true, Role = myRole });
    }   

    [HttpGet("getavatar")]
    public IActionResult GetAvatar()
    {
        var userClaims = HttpContext.User;
        string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";
        if (string.IsNullOrEmpty(userName)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Username tidak boleh kosong" : "Username could not be empty" });
        }
        var avatar = _userService.GetAvatar(userName!);
        return Ok(new { success = true, avatar = avatar });
    }        

    public class UsernameOrEmailExistsRequest
    {
        public string? userName { get; set; } = "";
        public string? email { get; set; } = "";
        public string? password { get; set; } = "";
    }
    [HttpPost("usernameoremailexists")]
    public IActionResult UsernameOrEmailExists([FromBody] UsernameOrEmailExistsRequest request)
    {
        Console.WriteLine("Cek username dan email");
        request.email = (request.email ?? "").Trim();
        request.userName = (request.userName ?? "").Trim();
        if (string.IsNullOrEmpty(request.email) || string.IsNullOrEmpty(request.userName)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Username dan Email tidak boleh kosong" : "Username and Email could not be empty" });
        }
        var dataExists = _userService.UsernameOrEmailExist(request.userName, request.email);
        return Ok(new { success = true, exists = dataExists });
    }   

    [HttpGet("userrole")]
    public IActionResult UserRole()
    {

        List<string> aRole = new List<string> { "Superuser", 
                "Administrator", 
                "Operator", "Kasir",
                "Lainnya" };
        return Ok(new { success = true, role = aRole });
    }


     public class UserUpdateData {
        public string? userName { get; set; } = "";
        public string? email  {get; set;} = "";
        public string? password { get; set; } = "";
        public DateOnly? dateOfBirth {get; set;} 
        public string? firstName { get; set; } = "";
        public string? lastName { get; set; } = "";
        public string? address { get; set; } = "";
        public string? address2 { get; set; } = "";
        public string? province { get; set; } = "";
        public string? city { get; set; } = "";
        public string? zipCode { get; set; } = "";
        public string? userRole { get; set; } = "Operator";        
        public int? isActive { get; set; }
        public IFormFile? avatarFile { get; set; } 

    }

    [HttpPost("addnewuser")]
    public async Task<IActionResult> AddNewUser([FromForm] UserUpdateData userData)
    {
        Console.WriteLine("Update AddNewUser(): " + userData.userName);
        userData.userName = userData.userName.Truncate(32);
        userData.email = userData.email.Truncate(32);
        userData.password = userData.password.Truncate(32);

        if (string.IsNullOrEmpty(userData.userName) || string.IsNullOrEmpty(userData.email) || string.IsNullOrEmpty(userData.password))
        {
            
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Username, email, atau password tidak boleh kosong" : 
                "Username, email, or password could not be empty" }    );
        } 

        string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$"; // Pola regex untuk email
        if (! Regex.IsMatch(userData.email, pattern)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Format email salah" : "Invalid email format" });
        };

        if (userData.userName.Length < 6 || userData.userName.Length > 20 || userData.password.Length < 6 || userData.password.Length > 20) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                        "Username dan password minimal harus berisi 6 - 20 karakter" : 
                        "Username and password must contain at least 6 - 20 characters" }    );
        }

        if (userData.email.Length < 11 || userData.email.Length > 50) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                "Email harus berisi 11 - 50 karakter" : "Email must contain 11 - 50 characters" }    );
        }

        pattern = @"^[a-zA-Z0-9]+$";
        if (! Regex.IsMatch(userData.userName, pattern)) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                "Username hanya boleh berisi huruf dan angka" : 
                "Usernames can only contain letters and numbers" }    );
        }               


        var isValid = !_userService.UsernameOrEmailExist(userData.userName, userData.email);
        if (! isValid) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                        "Username atau email sudah ada di database" : 
                        "Username or email already exists in the database" });
        }

        User UpdateUser = new User();

        // Simpan file Avatar
        if (userData.avatarFile != null && userData.avatarFile.Length > 0)
        {
            const long maxSizeInBytes = 30 * 1024;  // 30 Kb maksimal  ;
            if (userData.avatarFile.Length > maxSizeInBytes)
            {
                return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Ukuran file tidak boleh lebih dari 30 Kb" : "The file size should not exceed 30 Kb" });                
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(userData.avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Jenis yang didukung (jpg, jpeg, png, gif, webp)" : 
                                        "Supported file types (jpg, jpeg, png, gif, webp)" });                
            }

            // Periksa MIME
            var contentType = userData.avatarFile.ContentType;
            if (! contentType.StartsWith("image/"))
            {
                return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "File harus berupa gambar" :  "File must be an image" });                
            }


            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine("/images", "avatars", uniqueFileName);
            Console.WriteLine("Save Path : " + savePath);
            var publicUrl = Path.Combine(_env.WebRootPath, "images", "avatars", uniqueFileName);
            
            // Buat direktori kalau belum ada
            Console.WriteLine("Mencoba membuat direktori kalau belum ada");
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(publicUrl)!); 
            } catch {
                Console.WriteLine("Directori sudah ada lanjut saja");
            }
            Console.WriteLine("Simpan file : " + savePath );
            using var stream = new FileStream(publicUrl, FileMode.Create);
            await userData.avatarFile.CopyToAsync(stream);

            UpdateUser.Avatar200x200 = savePath;

            // Simpan nama file ke database kalau perlu            

        }        

        UpdateUser.IsActive = 1;
        UpdateUser.UserId = RandomStringGenerator.GenerateRandomString(20);
        UpdateUser.UserName = userData.userName;
        UpdateUser.Email = userData.email;
        UpdateUser.Password = userData.password;
        UpdateUser.FirstName = userData.firstName.Truncate(32);
        UpdateUser.LastName =  userData.lastName.Truncate(32);
        UpdateUser.UserRole = userData.userRole.Truncate(20);
        UpdateUser.Address = userData.address.Truncate(40);
        UpdateUser.Address2 = userData.address2.Truncate(40);
        UpdateUser.Province = userData.province.Truncate(20);
        UpdateUser.City = userData.city.Truncate(20);
        UpdateUser.ZipCode = userData.zipCode.Truncate(6);
        UpdateUser.DateOfBirth = userData.dateOfBirth ?? DateOnly.FromDateTime(DateTime.Today);
        UpdateUser.UserRole = userData.userRole;
        UpdateUser.IsActive = userData.isActive; 
        UpdateUser.TermsAgrement =  1; 

        var userDataDto = await _userService.AddAsync(UpdateUser);
        return Ok(new { success = true, message = (_lang.CurrentLang == "id") ? 
                                        "Data sudah disimpan" : "User updated successfully", user = userDataDto });

    }


    [HttpPost("updateuser")]
    public async Task<IActionResult> UpdateUser([FromForm] UserUpdateData userData)
    {
        Console.WriteLine("Update UpdateUser(): " + userData.userName);
        userData.userName = userData.userName.Truncate(32);
        var UpdateUser = _userService.GetUser(userData.userName!, true);
        if (UpdateUser == null || UpdateUser.IsEmpty)
        {
            return NotFound(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "Data user " + userData.userName + " tidak ditemukan" : "User " + userData.userName + " not found" });
        }
        if (UpdateUser.IsRoot == 1) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "User ini tidak boleh diubah" : "Update not allowed" });
        }

        userData.firstName = userData.firstName.Truncate(32);
        userData.lastName =  userData.lastName.Truncate(32);
        userData.userRole = userData.userRole.Truncate(20);

        Console.WriteLine("Cek image avatar");
        if (userData.avatarFile != null && userData.avatarFile.Length > 0)
        {
            Console.WriteLine("Validasi simpan image avatar");

            const long maxSizeInBytes = 30 * 1024;  // 30 Kb maksimal  [1024 * 1024 = 1Mb];
            if (userData.avatarFile.Length > maxSizeInBytes)
            {
                return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Ukuran file tidak boleh lebih dari 100 Kb" : "The file size should not exceed 100 Kb" });                
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(userData.avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "Jenis yang didukung (jpg, jpeg, png, gif, webp)" : 
                                        "Supported file types (jpg, jpeg, png, gif, webp)" });                
            }

            // Periksa MIME
            var contentType = userData.avatarFile.ContentType;
            if (! contentType.StartsWith("image/"))
            {
                return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ? 
                                        "File harus berupa gambar" :  "File must be an image" });                
            }

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";

            var savePath = Path.Combine("/images", "avatars", uniqueFileName);
            Console.WriteLine("Save Path : " + savePath);
            var publicUrl = Path.Combine(_env.WebRootPath, "images", "avatars", uniqueFileName);
            

            // Buat direktori kalau belum ada
            Console.WriteLine("Mencoba membuat direktori kalau belum ada");
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(publicUrl)!); 
            } catch {
                Console.WriteLine("Directori sudah ada lanjut saja");
            }
            Console.WriteLine("Simpan file : " + savePath );
            using var stream = new FileStream(publicUrl, FileMode.Create);
            await userData.avatarFile.CopyToAsync(stream);


            /* ============================================
            | Bagian ini untuk menyimpan gambar Avatar ke Supabase.com kalau server tidak
            | memungkinkan menyimpan gambar
            | Ganti :
            |    1. <url> dengan url storage dari supabase 
            |    2. <anon.key> dengan key yang didapat dari Supabase.com
            =============================================== */             
            /* var fileName = Guid.NewGuid() + Path.GetExtension(userData.avatarFile.FileName);
            Console.WriteLine("Persiapan simpan image avatar : " + fileName);

            // Supabase info (sebaiknya simpan di environment variable)
            var supabaseProjectUrl = "<url>";
            var supabaseAnonKey = "<anon.key>";
            var bucketName = "avatars";

            var uploadUrl = $"{supabaseProjectUrl}/storage/v1/object/{bucketName}/{fileName}";

            Console.WriteLine("Step 1 declare httpclient Upload URL : " + uploadUrl);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", supabaseAnonKey);
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(userData.avatarFile.OpenReadStream());
            content.Add(streamContent, "file", fileName);

            var response = await client.PostAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Upload failed: " + error);
                return StatusCode((int)response.StatusCode, $"Upload gagal: {error}");
            }
            var publicUrl = $"{supabaseProjectUrl}/storage/v1/object/public/{bucketName}/{fileName}";            
            Console.WriteLine($"File disimpan sebagai:{publicUrl}"); */

            UpdateUser.Avatar200x200 = savePath;

            // Simpan nama file ke database kalau perlu            

        }
        UpdateUser.DateOfBirth = userData.dateOfBirth ?? DateOnly.FromDateTime(DateTime.Today);
        UpdateUser.FirstName = userData.firstName; 
        UpdateUser.LastName = userData.lastName;
        UpdateUser.Address = userData.address.Truncate(40);
        UpdateUser.Address2 = userData.address2.Truncate(40);
        UpdateUser.Province = userData.province.Truncate(20);
        UpdateUser.City = userData.city.Truncate(20);
        UpdateUser.ZipCode = userData.zipCode.Truncate(6);
        UpdateUser.UserRole = userData.userRole;
        UpdateUser.IsActive = userData.isActive;        
        // Simpan data user ke database
        var userDataDto = await _userService.UpdateAsync(UpdateUser);

        return Ok(new { success = true, message = (_lang.CurrentLang == "id") ? 
                                        "Data sudah disimpan" : "User updated successfully", user = userDataDto });
    }

    public class UserResetPassword
    {
        public string userName { get; set; } = "";
        public string? oldPassword { get; set; } = "";
        public string? newPassword { get; set; } = "";
    }

    [HttpPost("deleteuser")]
    public async Task<IActionResult> DeleteUser([FromBody] UserResetPassword request)
    {
        Console.WriteLine("Delete user: " + request.userName);
        var DeleteUser = _userService.GetUser(request.userName, false);
        if (DeleteUser == null)
        {
            return NotFound(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "Data user tidak ditemukan" : "User not found" });
        } 
        Console.WriteLine("Is Root : ", DeleteUser.IsRoot);
        if (DeleteUser.IsRoot == 1) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "User ini tidak boleh dihapus" : "Delete not allowed" });
        }
        Console.WriteLine("Menghapus user : " + request.userName);
        await _userService.DeleteUserAsync(request.userName);
        return Ok(new { success = true, message = (_lang.CurrentLang == "id") ? 
                                        "Data sudah dihapus" : "User deleted successfully" });
    }

    [HttpPost("changepassword")]
    public async Task<IActionResult> ChangePassword([FromForm] UserResetPassword request)
    {
        Console.WriteLine("Change password: " + request.userName);
        
        request.userName = (request.userName ?? "").Trim();
        request.oldPassword = (request.oldPassword ?? "").Trim();
        request.newPassword = (request.newPassword ?? "").Trim();
        
        var UpdateUser = _userService.GetUser(request.userName, true);
        if (UpdateUser == null)
        {
            return NotFound(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "Data user tidak ditemukan" : "User not found" });
        }
        var userClaims = HttpContext.User;
        string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";
        if (userName != request.userName) {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "Anda tidak berhak mengubah password orang ini" : 
                                        "You are not allowed to change other password." });

        }
        var userValid = _userService.VerifyPassword(request.oldPassword!, UpdateUser.Password!); 
        if (!userValid)
        {
            return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "Password lama salah" : "Old password is incorrect" });
        }
        UpdateUser.Password = _userService.HashPassword(request.newPassword!);
        await _userService.UpdateAsync(UpdateUser);
        return Ok(new { success = true, message = (_lang.CurrentLang == "id") ? 
                                        "Perubahan sudah disimpan" : "Password saved" });
    }

    [HttpPost("overidepassword")]
    public async Task<IActionResult> OveridePassword([FromForm] UserResetPassword request)
    {
        
        request.userName = (request.userName ?? "").Trim();
        request.newPassword = (request.newPassword ?? "").Trim();

        var UpdateUser = _userService.GetUser(request.userName, true);
        if (UpdateUser == null)
        {
            return NotFound(new { success = false, message = (_lang.CurrentLang == "id") ?
                                        "Data user tidak ditemukan" : "User not found" });
        }
        if (UpdateUser.IsRoot==1) {
            //var userClaims = HttpContext.User;
            //string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";
            //if (userName != request.userName) {
                return BadRequest(new { success = false, message = (_lang.CurrentLang == "id") ?
                                            "Anda tidak berhak mengubah password Administrator" : 
                                            "You are not allowed to change Administrator password." });
            //}
        }
        UpdateUser.Password = _userService.HashPassword(request.newPassword!);
        await _userService.UpdateAsync(UpdateUser);
        return Ok(new { success = true, message = (_lang.CurrentLang == "id") ? 
                                        "Password sudah disimpan" : "Password saved" });
    }                          
                              
}
