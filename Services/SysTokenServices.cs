using AspApi.Data;
using AspApi.Models;
using Microsoft.EntityFrameworkCore;
//using BCrypt.Net;

namespace AspApi.Services
{
  public class SysTokenService
  {

    private readonly DataContext _context = default!;

    public SysTokenService(DataContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> CountAsync()
    {
      return await (_context?.SysTokens?.CountAsync() ?? Task.FromResult(0));
    }

    public IList<SysToken> GetTokens()
    {
      if (_context.SysTokens != null)
      {
        return _context.SysTokens.ToList();
      }
      return new List<SysToken>();
    }


    public SysToken GetTokenInfo(string accesstoken, string refreshtoken = "")
    {
        return _context.SysTokens?
            .FirstOrDefault(x =>
                x.AccessToken == accesstoken && (string.IsNullOrEmpty(refreshtoken) || x.RefreshToken == refreshtoken))!;
    }    

    public bool IsTokenValid(string accesstoken, string refreshtoken)
    {
      if (_context.SysTokens != null)
      {
        //Console.WriteLine("Access token : " + accesstoken + " | Refresh token : " + refreshtoken);
        //var tokenValid = _context.SysTokens.Count(x => x.AccessToken == accesstoken && x.RefreshToken == refreshtoken && x.IsExpired < 1) > 0;
        var tokenValid = _context.SysTokens.Count(x => x.AccessToken == accesstoken && x.RefreshToken == refreshtoken) > 0; 
        return tokenValid;
      }
      return false;
    }

    public bool AddToken(SysToken token)
    {
      if (_context.SysTokens != null)
      {
        _context.SysTokens.Add(token);
        _context.SaveChanges();
        return true;
      }
      return false;
    }
    
    public void DeleteTokenByAccessToken(string accesstoken)
    {
      if (_context.SysTokens != null)
      {
        var token = _context.SysTokens.FirstOrDefault(x => x.AccessToken == accesstoken && x.IsExpired == 0);
        if (token != null) {
          _context.SysTokens.Remove(token);
          _context.SaveChanges();
        }
      }
    }

    public bool DisactivateToken(SysToken sysToken)
    {
      if (_context.SysTokens != null)
      {
        //Console.WriteLine("Hapus token");
        //SysToken sysToken = _context.SysTokens.Where(x => x.AccessToken == accesstoken && x.RefreshToken == refreshtoken).FirstOrDefault()!;
        if (sysToken != null) {
          //_context.SysTokens.Remove(sysToken);
          sysToken.IsExpired = 1;
          _context.SaveChanges();
          return true;
        }
      }
      return false;
    }

  }
}
