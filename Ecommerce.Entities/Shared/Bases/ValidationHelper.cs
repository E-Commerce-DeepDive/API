using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.Shared.Bases
{
    public static class ValidationHelper
    {
        public static string FlattenErrors(IEnumerable<ValidationFailure> failures)
        {
            return string.Join("; ", failures.Select(f => f.ErrorMessage).Distinct());
        }
    }
}
