using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primbot_v._2.Modules.Control {
    /// <summary>
    /// Toggle modules on or off. All are on by default. Administration control.
    /// </summary>
    public class Permit {
        ulong ownerID = 263733973711192064;

        //(command,roleID) - wild cards: everyone, nobody, owner
        Dictionary<string, string> Permissions = new Dictionary<string, string> {
            { "permit","owner" },
            { "say","everyone" },
            { "ping","everyone" },
            { "help","everyone" },
            { "bananaboy","everyone" },
            { "react","everyone" },
            { "gooutside","everyone" },
            { "event","everyone" },
            { "8ball","everyone" }
        };




    }
}
