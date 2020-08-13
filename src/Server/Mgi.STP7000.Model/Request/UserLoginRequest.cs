using System.ComponentModel.DataAnnotations;

namespace Mgi.STP7000.Model.Request
{
    public class UserLoginRequest
    {
        [Required]
        [MaxLength(20)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(60)]
        public string Password { get; set; }
    }
}
