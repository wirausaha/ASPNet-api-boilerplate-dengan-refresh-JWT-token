using AspApi.Dtos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspApi.Models
{
    [Table("users")]
    public class User
    {
        public User()
        {
            IsActive = 1;
            IsEmailConfirmed = 0;
            IsPhoneConfirmed = 0;
            ParentUserName = "";
            RootUserName = "";
            BaseLocale = "id-ID";
            BaseLanguage = "id";
        }

        [Key]
        //[Required(ErrorMessage = "Username harus diisi")]
        [StringLength(32, ErrorMessage = "Username tidak boleh lebih dari 32 karakter")]
        public string? UserId { get; set; } = "";

        public string? UserName { get; set; } = "";

        //[Required(ErrorMessage = "Format email salah")]
        [StringLength(32, ErrorMessage = "Email  tidak boleh lebih dari 32 karakter")]
        //[EmailAddress(ErrorMessage = "Format email salah")]
        public string? Email { get; set; } = "";

        //[Required(ErrorMessage = "Password harus diisi")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password minimal 6 karakter maksimal 20 karakter")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        //      ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string? Password { get; set; } = "";

        [StringLength(40, ErrorMessage = "Telepon maksimal 20 karakter")]
        public string? PhoneNumber { get; set; } = "";

        [StringLength(40, ErrorMessage = "First name maksimal 32 karakter")]
        public string? FirstName { get; set; } = "";

        [StringLength(40, ErrorMessage = "Last name maksimal 32 karakter")]
        public string? LastName { get; set; } = "";

        // [Required(ErrorMessage = "Date of Birth is required")]
        //[DataType(DataType.Date, ErrorMessage = "Invalid date format")]
        //[CustomValidation(typeof(User), nameof(ValidateAge))]
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(40, ErrorMessage = "Alamat maksimal 40 karakter")]
        public string? Address { get; set; } = "";
        public string? Address2 { get; set; } = "";

        [StringLength(20, ErrorMessage = "Propinsi maksimal 20 karakter")]
        public string? Province { get; set; } = "";

        [StringLength(20, ErrorMessage = "Kota maksimal 20 karakter")]
        public string? City { get; set; } = "";

        [StringLength(6, ErrorMessage = "Kode Pos maksimal 6 karakter")]
        public string? ZipCode { get; set; } = "";

        [StringLength(32, ErrorMessage = "Parent / root user name cannot be longer than 32 characters")]
        public string? RootUserName { get; set; } = "";
        public string ParentUserName { get; set; } = "";

        public string? Avatar200x200 { get; set; } = "";
        public string? BaseLocale { get; set; } = "id-ID";
        public string? BaseLanguage { get; set; } = "id";        

        public int? IsActive { get; set; }
        public int? IsEmailConfirmed { get; set; }
        public int? IsPhoneConfirmed { get; set; }

        public int TermsAgrement { get; set; } = 1;

        public int IsRoot { get; set; } = 0;
        public string? UserRole { get; set; } = "User";

        public string? MembershipId { get; set; } = "000";

        [NotMapped]
        public bool IsEmpty => (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Email)) ;

    }


    [NotMapped]
    public class UserDataTables {
      public int draw { get; set; } = 0;
      public int recordsTotal { get; set; } = 0;
      public int recordsFiltered { get; set; } = 0;
      public List<UserDataDtos> data { get; set; } = new List<UserDataDtos>();
    }

}
