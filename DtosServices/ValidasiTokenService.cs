using AspApi.Data;
using System.Security.Claims;

namespace AspApi.DTOServices
{
    public class ValidasiTokenService 
    {
        private readonly DataContext _context = default!;

        public ValidasiTokenService(DataContext context)
        {
            _context = context;
        }

        public bool TokenValid(ClaimsPrincipal userClaims)
        {
            if (userClaims == null)
            {
                Console.WriteLine(DateTime.Now.ToString() + " : Error TokenValid() Claims= null");
                return false;
            } 
            string userName = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? "";
            string email = userClaims.FindFirst(ClaimTypes.Email)?.Value ?? "";
            string companyCode = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0000";
            Console.WriteLine(userName + " | " + email + " | " + companyCode);
            if (userName == "" || email == "" || companyCode == "")
            {
                Console.WriteLine(DateTime.Now.ToString() + " : Error validasi token, unauthorized");
                return false;
            }             
            return true;  
        }
    }   
}
