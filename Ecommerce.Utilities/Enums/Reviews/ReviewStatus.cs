using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Utilities.Enums.Reviews
{
    public enum ReviewStatus
    {
        Pending,
        Approved,
        Rejected,   // Final rejection, no edit allowed
        Edited      // Admin rejected but buyer can edit/resubmit
    }
}
