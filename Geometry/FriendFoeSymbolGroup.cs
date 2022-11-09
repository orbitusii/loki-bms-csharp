using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loki_bms_csharp.Database;

namespace loki_bms_csharp.Geometry
{
    public class FriendFoeSymbolGroup
    {
        public Dictionary<FriendFoeStatus, SVG.SVGPath> Symbols;

        public FriendFoeSymbolGroup (IEnumerable<SVG.SVGPath> collection)
        {
            Symbols = new Dictionary<FriendFoeStatus, SVG.SVGPath>()
            {
                { FriendFoeStatus.KnownFriend, null },
                { FriendFoeStatus.AssumedFriend, null },
                { FriendFoeStatus.Neutral, null },
                { FriendFoeStatus.Suspect, null },
                { FriendFoeStatus.Hostile, null },
                { FriendFoeStatus.Unknown, null },
                { FriendFoeStatus.Pending, null },
            };

            foreach (SVG.SVGPath p in collection)
            {
                var ffs = MatchFFSFromString(p.name);

                Symbols[ffs] = p;
            }
        }

        private FriendFoeStatus MatchFFSFromString (string str)
        {
            return str.ToLower() switch
            {
                "fnd" => FriendFoeStatus.KnownFriend,
                "asf" => FriendFoeStatus.AssumedFriend,
                "neu" => FriendFoeStatus.Neutral,
                "sus" => FriendFoeStatus.Suspect,
                "hos" => FriendFoeStatus.Hostile,
                "unk" => FriendFoeStatus.Unknown,
                _ => FriendFoeStatus.Pending,
            };
        }

        public SVG.SVGPath this[FriendFoeStatus index]
        {
            get => Symbols[index];
        }
    }
}
