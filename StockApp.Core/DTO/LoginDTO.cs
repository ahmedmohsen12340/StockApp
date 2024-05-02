using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Core.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage ="{0} Can't be Empty")]
        [EmailAddress(ErrorMessage ="Input A valid Email Address")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "{0} Can't be Empty")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
