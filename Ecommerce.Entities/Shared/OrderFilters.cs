using Ecommerce.Utilities.Enums;

namespace Ecommerce.Entities.DTO.Shared;
public class OrderFilters<TSortColumn> : RequestFilters<TSortColumn>
    where TSortColumn : struct, Enum

{
    public OrderStatus? Status { get; set; }
}
