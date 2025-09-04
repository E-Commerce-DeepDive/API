using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Account
{
    public class UpdateUserProfileDto
    {
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
