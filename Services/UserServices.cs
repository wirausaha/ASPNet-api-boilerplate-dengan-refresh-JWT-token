using AspApi.Data;
using AspApi.Models;
using AspApi.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Linq;


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

    public int Count()
    {
      var intRetval = _context.Users.Count();
      return intRetval;
    }    

    // Mengambil daftar user
    public IList<User> GetUsers()
    {
      if (_context.Users != null)
      {
        return _context.Users.ToList();
      }
      return new List<User>();
    }

    // Mengambil 1 user berdasarkan username atau email
    public User GetUser(string usernameOrEmail, bool forUpdate = true)
    {
      if (_context.Users != null)
      {
        if (forUpdate)
        {
          // Untuk update, ambil user yang aktif
          return _context.Users.FirstOrDefault(x => x.IsActive == 1 && (x.UserName == usernameOrEmail || x.Email == usernameOrEmail)) ?? new User();
        }
        return _context.Users.AsNoTracking().FirstOrDefault(x => x.IsActive == 1 && (x.UserName == usernameOrEmail || x.Email == usernameOrEmail)) ?? new User();
      }
      return new User();
    }


    public string GetAvatar(string usernameOrEmail)
    {
      if (_context.Users != null)
      {         
        return _context.Users.Where(x => x.IsActive == 1 && (x.UserName == usernameOrEmail || x.Email == usernameOrEmail))
            .Select(x => x.Avatar200x200).FirstOrDefault() ?? "";
      }
      return "";
    }

    public string GetMyRole(string usernameOrEmail)
    {
      if (_context.Users != null)
      {         
        return _context.Users.Where(x => x.IsActive == 1 && (x.UserName == usernameOrEmail || x.Email == usernameOrEmail))
            .Select(x => x.UserRole).FirstOrDefault() ?? "";
      }
      return "";
    }    

    /* ==============================================================
    | GetUserWithPagination()
    | Mengambil daftar user dengan pagination untuk DataTables menggunakan Dapper
    | 
    =================================================== */    
    public UserDataTables GetUserWithPagination(int draw, 
              int start, int length, string? filter = null, DateTime? lastUpdate = null) {

        var retList = new UserDataTables(){
              draw = draw,
              recordsTotal = 0,
              recordsFiltered = 0,
              data = new List<UserDataDtos>()
          };
        var userList = _context.Users
            .Where(u => string.IsNullOrEmpty(filter) || u.UserName!.Contains(filter))
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Skip(start)
            .Take(length)
            .Select(u => new UserDataDtos(
                u.UserId!, u.UserName!, u.Email!, u.PhoneNumber!,
                u.FirstName!, u.LastName!, u.DateOfBirth!, u.Address!,
                u.Address2!, u.Province!, u.City!, u.ZipCode!, u.Avatar200x200!, u.UserRole!,
                u.IsActive, u.IsEmailConfirmed, u.IsPhoneConfirmed
            ))
            .ToList();


        int totalRecords = _context.Users.Count();
        int filteredRecords = _context.Users
            .Where(u => string.IsNullOrEmpty(filter) || u.UserName!.Contains(filter))
            .Count();

        retList.recordsTotal = totalRecords;
        retList.recordsFiltered = filteredRecords;
        retList.data = userList;
          //Console.WriteLine("Jumlah data: " + result.Count);            
        return retList;
      
    }            

    public UserDataDtos GetUserDataDtos(string userName) {
        if (_context.Users == null || string.IsNullOrEmpty(userName)) return new UserDataDtos();
        var userdataDtos = _context.Users
            .Where(u => u.UserName! == userName)
            .AsNoTracking()
            .Select(u => new UserDataDtos(
                u.UserId!, u.UserName!, u.Email!, u.PhoneNumber!,
                u.FirstName!, u.LastName!, u.DateOfBirth!, u.Address!,
                u.Address2!, u.Province!, u.City!, u.ZipCode!, u.Avatar200x200!, u.UserRole!,
                u.IsActive, u.IsEmailConfirmed, u.IsPhoneConfirmed
            )).FirstOrDefault();

        return userdataDtos!;
      
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
      return _context?.Users?.Any(x => x.Email == email) ?? false;
    }

    public Boolean UsernameOrEmailExist(string userName, string email)
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

    public async Task<UserDataDtos> AddAsync(User user)
    {
      if (_context.Users != null)
      {
        if (! string.IsNullOrEmpty(user.Password))
        {
          user.Password = HashPassword(user.Password!);
          _context.Users.Add(user);
          await _context.SaveChangesAsync();
          var userdataDtos = await _context.Users
              .Where(u => u.UserName! == user.UserName)
              .AsNoTracking()
              .Select(u => new UserDataDtos(
                  u.UserId!, u.UserName!, u.Email!, u.PhoneNumber!,
                  u.FirstName!, u.LastName!, u.DateOfBirth!, u.Address!,
                  u.Address2!, u.Province!, u.City!, u.ZipCode!, u.Avatar200x200!, u.UserRole!,
                  u.IsActive, u.IsEmailConfirmed, u.IsPhoneConfirmed
              )).FirstOrDefaultAsync();
          return userdataDtos!;
        }
      }
      return new UserDataDtos();
    }

    public async Task<UserDataDtos> UpdateAsync(User user)
    {
      //Console.WriteLine("User Update service");
      if (_context.Users != null)
      {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var userdataDtos = await _context.Users
            .Where(u => u.UserName! == user.UserName)
            .AsNoTracking()
            .Select(u => new UserDataDtos(
                u.UserId!, u.UserName!, u.Email!, u.PhoneNumber!,
                u.FirstName!, u.LastName!, u.DateOfBirth!, u.Address!,
                u.Address2!, u.Province!, u.City!, u.ZipCode!, u.Avatar200x200!, u.UserRole!,
                u.IsActive, u.IsEmailConfirmed, u.IsPhoneConfirmed
            )).FirstOrDefaultAsync();
        return userdataDtos!;
      } else {
        return new UserDataDtos();
      }
    }

    public async Task DeleteUserAsync(string userName)
    {
      if (_context.Users != null)
      {
        User user = _context.Users.Where(a => a.UserName == userName).FirstOrDefault()!;
        if (user != null) {
          _context.Users.Remove(user);
          //user.IsActive = 0;
          //_context.Users.Update(user);
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
