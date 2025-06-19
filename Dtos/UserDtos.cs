using System.ComponentModel.DataAnnotations.Schema;

namespace AspApi.Dtos
{
    [NotMapped]
    public class UserDataDtos {
        public string? UserId { get; set; } = "";
        public string? UserName { get; set; } = "";
        public string? Email { get; set; } = "";
        public string? PhoneNumber { get; set; } = "";
        public string? FirstName { get; set; } = "";
        public string? LastName { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; } = "";
        public string? Address2 { get; set; } = "";
        public string? Province { get; set; } = "";
        public string? City { get; set; } = "";
        public string? ZipCode { get; set; } = "";
        public string? Avatar200x200 { get; set; } = "";        
        public string? UserRole { get; set; } = "User";        
        public int? IsActive { get; set; }
        public int? IsEmailConfirmed { get; set; }
        public int? IsPhoneConfirmed { get; set; }        

        public UserDataDtos() {
        }

        public UserDataDtos(string userid, string username, string email, 
                            string phoneNumber, string firstName, string lastName,
                            DateTime? dateOfBirth, string address, string address2,
                            string province, string city, string zipCode,
                            string avatar200x200, string userrole, int? isActive = 1,
                            int? isEmailConfirmed = 0, int? isPhoneConfirmed = 0) 
        {
            UserId = userid;
            UserName = username;
            Email = email + ""; // saya lebih suka memaksa jadi string daripada null
            PhoneNumber = phoneNumber + "";
            FirstName = firstName + "";
            LastName = lastName + "";
            DateOfBirth = dateOfBirth!;
            Address = address + "";
            Address2 = address2 + "";
            Province = province + "";
            City = city + "";
            ZipCode = zipCode + "";
            Avatar200x200 = avatar200x200;
            UserRole = userrole ?? "User"; 
            IsActive = isActive;
            IsEmailConfirmed = isEmailConfirmed;
            IsPhoneConfirmed = isPhoneConfirmed;
        }

    }

}

    