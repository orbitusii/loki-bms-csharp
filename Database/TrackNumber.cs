using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public abstract class TrackNumber: IEquatable<TrackNumber>
    {
        public string Type => this switch
        {
            Internal => "Internal",
            External => "External",
            DataLink => "Datalink",
            _ => "Unknown"
        };
        public short Value { get; set; }

        public TrackNumber() { }

        public class Internal : TrackNumber { }
        public class External : TrackNumber { }
        public class DataLink : TrackNumber { }

        public bool Equals(TrackNumber other)
        {
            Type myType = this.GetType();
            if (other.GetType() == myType && other.Value == this.Value)
            {
                return true;
            }
            else return false;
        }

        public static void Test ()
        {
            var itn0 = new Internal { Value = 0, };
            var itn1 = new Internal { Value = 1, };

            var etn0 = new External { Value = 0, };
            var etn1 = new External { Value = 0, };

            System.Diagnostics.Debug.Assert(!itn0.Equals(itn1));
            System.Diagnostics.Debug.Assert(!itn0.Equals(etn0));
            System.Diagnostics.Debug.Assert(etn0.Equals(etn1));
            System.Diagnostics.Debug.WriteLine("[UNIT TEST] TrackNumber Equatability test passed");
        }
    }
}
