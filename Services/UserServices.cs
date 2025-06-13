using AspApi.Data;
using AspApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AspApi.Services
{
  public class UserService
  {

    private readonly DataContext _context = default!;

    public UserService(DataContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> CountAsync()
    {
      return await (_context?.Users?.CountAsync() ?? Task.FromResult(0));
    }

    public IList<User> GetUsers()
    {
      if (_context.Users != null)
      {
        return _context.Users.ToList();
      }
      return new List<User>();
    }

    public User GetUser(string usernameOrEmail)
    {
      if (_context.Users != null)
      {
        return _context.Users.FirstOrDefault(x => x.IsActive == 1 && (x.UserName == usernameOrEmail || x.Email == usernameOrEmail)) ?? new User();
      }
      return new User();
    }


    public string getFullName(string firstname, string LastName)
    {
      if (firstname + LastName == "") return "Belum ada nama";
      return $"{firstname.Trim()} {LastName.Trim()}";
    }

    public Boolean UserExist(string UserName)
    {
      return _context?.Users?.Any(x => x.UserName == UserName) ?? false;
    }

    public string GetUserIdByUserName(string UserName)
    {
      return _context?.Users?
          .Where(x => x.UserName == UserName)
          .Select(x => x.UserId)
          .FirstOrDefault() ?? "";
    }

    public string GetUserEmailByUserName(string UserName)
    {
      return _context?.Users?
          .Where(x => x.UserName == UserName)
          .Select(x => x.Email)
          .FirstOrDefault() ?? "";
    }

    public Boolean UserIsRoot(string userName)
    {
      return _context?.Users?.Any(x => x.UserName == userName && x.IsRoot == 1 ) ?? false;
    }

    public Boolean EmailExist(string email)
    {
      //Console.WriteLine("User Service : Cek Email Exist ");
      return _context?.Users?.Any(x => x.Email == email) ?? false;
    }

    // Periksa apakah username atau email sudah ada
    public Boolean UsernameAndEmailExist(string userName, string email)
    {
      return _context?.Users?.Any(x => x.UserName == userName || x.Email == email) ?? false;
    }

    public Boolean ValidUser(string UserName, string Password)
    {

      //Console.WriteLine(UserName + " | " + Password);
      if (Password == string.Empty) return false;
      if (_context.Users != null)
      {
        User user = _context.Users.FirstOrDefault(x => x.IsActive == 1 && (x.UserName == UserName || x.Email == UserName)) ?? new User();
        if (! user.IsEmpty)
        {
          // Console.WriteLine("User Service : User found");
          if (VerifyPassword(Password, user.Password!)) return true;
        }
      }
      return false;
    }


    public string HashPassword(string password)
    {
      return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
      return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public bool AddUser(User user)
    {
      if (_context.Users != null)
      {
        if (string.IsNullOrEmpty(user.Password))
        {
          return false;
        }
        user.Password = HashPassword(user.Password!);
        _context.Users.Add(user);
        _context.SaveChanges();
        return true;
      }
      return false;
    }

    public void ResetPassword(string userName, string password)
    {
        var hashedPassword = HashPassword(password!);
        var sql = "UPDATE Users SET Password = @p0 WHERE UserName = @p1";
        _context.Database.ExecuteSqlRaw(sql, hashedPassword, userName);        
    }

    public async Task UpdateAsync(User user)
    {
      //Console.WriteLine("User Update service");
      if (_context.Users != null)
      {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
      }
    }

    public async Task DeleteUserAsync(string userName)
    {
      if (_context.Users != null)
      {
        User user = _context.Users.Where(a => a.UserName == userName).FirstOrDefault()!;
        if (user != null) {
          user.IsActive = 0;
          _context.Users.Update(user);
          await _context.SaveChangesAsync();
        }
      }
    }

    public async Task UpdateParentUser(string userName, string parentUserName)
    {
        User user = _context?.Users?.SingleOrDefault(u => u.UserName == userName)!;
        if (user != null)
        {
            user.ParentUserName = parentUserName;
            await _context!.SaveChangesAsync();
        }
    }    

    public async Task SetAsRoot(string userName)
    {
        User user = _context?.Users?.SingleOrDefault(u => u.UserName == userName)!;
        if (user != null)
        {
            user.IsRoot = 1;
            await _context!.SaveChangesAsync();
        }
    }    

  }
}
