using Microsoft.AspNetCore.Mvc;
using StockApp.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Core.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage ="{0} is Required")]
        public string? PersonName { get; set; }
        [Required(ErrorMessage = "{0} is Required")]
        [EmailAddress(ErrorMessage ="Enter Valid E_Mail address")]
        [DataType(DataType.EmailAddress)]
        [Remote("IsEmailRegistered","Account")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "{0} is Required")]
        [Phone(ErrorMessage ="Enter Valid Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? Phone {  get; set; }
        [Required(ErrorMessage = "{0} is Required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required(ErrorMessage = "{0} is Required")]
        [Compare("Password",ErrorMessage = "Confirm Password is incorrect")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
        [Required(ErrorMessage = "{0} is Required")]
        public UserTypeOptions UserTypeOptions { get; set; } = UserTypeOptions.User;
    }
}
