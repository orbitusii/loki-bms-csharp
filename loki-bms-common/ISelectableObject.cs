using loki_bms_common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_common
{
    public interface ISelectableObject : IPositionedObject {
        public FriendFoeStatus FFS { get; set; }
    }
}
