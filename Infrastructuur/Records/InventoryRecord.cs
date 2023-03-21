using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Records
{
    public record InventoryItemMapParams(int Id, int CategoryId, int SupplierId, int ProductId, int Quantity);
}
