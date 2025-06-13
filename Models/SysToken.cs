using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspApi.Models
{
  [Table("tbsystoken")]
  public class SysToken
  {
    [Key]
    public int Id { get; set; }
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string CompanyCode { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserRole { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime ExpireDate { get; set; }
    public int IsExpired {get; set;} = 0;

    public SysToken() {}
    public SysToken(string accesstoken, string refreshtoken, 
        string companyCode, string userId, string userRole, string email)
    {
      AccessToken = accesstoken;
      RefreshToken = refreshtoken;
      CompanyCode = companyCode;
      UserId = userId;
      UserRole = userRole;
      Email = email;
      ExpireDate = DateTime.Now.AddDays(7);
      IsExpired = 0; 
    } 

  }
  
  [NotMapped]
  public class TokenDataTables {
      public int draw { get; set; } = 0;
      public int recordsTotal { get; set; } = 0;
      public int recordsFiltered { get; set; } = 0;
      public List<SysToken> data { get; set; } = new List<SysToken>();
  }

}


