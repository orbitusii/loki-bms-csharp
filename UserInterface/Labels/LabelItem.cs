using loki_bms_common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_csharp.UserInterface.Labels
{
    public abstract partial class LabelItem
    {
        // Default constructor - this class is abstract, basically a factory, so you shouldn't be able to make a random LabelItem object!
        private LabelItem() { }

        public abstract string Evaluate(ISelectableObject target);

        public LabelItem? GetLabelForProperty (string propertyName)
        {
            return propertyName.ToLower() switch
            {
                "altitude" or "alt" => new AltitudeLabel(),
                _ => null
            };
        }

        [LabelItem("Separator", LabelItemAttribute.TargetClasses.Everything)]
        public class LabelSeparator : LabelItem
        {
            public override string Evaluate(ISelectableObject target)
            {
                return " / ";
            }
        }

        [LabelItem("Line Break", LabelItemAttribute.TargetClasses.Everything)]
        public class LabelNewLine : LabelItem
        {
            public override string Evaluate(ISelectableObject target)
            {
                return "\n";
            }
        }

        public class CustomText : LabelItem
        {
            public string Text = string.Empty;
            public override string Evaluate(ISelectableObject target)
            {
                return Text;
            }
        }

        [LabelItem("Name", LabelItemAttribute.TargetClasses.Everything)]
        public class NameLabel : LabelItem
        {
            public override string Evaluate(ISelectableObject target)
            {
                string name = "-";

                if(target is TrackFile tf)
                {
                    if(tf.Callsign != string.Empty)
                        name = tf.Callsign;
                }
                if(target is TacticalElement te)
                {
                    if (te.Name != string.Empty)
                        name = te.Name;
                }

                return name;
            }
        }

        [LabelItem("Altitude", LabelItemAttribute.TargetClasses.TrackFile)]
        public class AltitudeLabel : LabelItem
        {
            public bool ImperialUnits = true;
            public string format = "000";

            public override string Evaluate(ISelectableObject target)
            {
                if (target is TrackFile tf)
                {
                    double altValue = ImperialUnits ? tf.Altitude * Conversions.MetersToFeet : tf.Altitude;

                    return (altValue / 100).ToString(format);
                }

                return "-";
            }
        }

        [LabelItem("Position", LabelItemAttribute.TargetClasses.Everything)]
        public class PositionLabel : LabelItem
        {
            public override string Evaluate(ISelectableObject target)
            {
                LatLonCoord latlon = Conversions.XYZToLL(target.Position);

                return latlon.ToString();
            }
        }

        [LabelItem("Internal TN", LabelItemAttribute.TargetClasses.TrackFile)]
        public class TNLabel : LabelItem
        {
            public override string Evaluate(ISelectableObject target)
            {
                if (target is TrackFile tf)
                {
                    TrackNumber? itn = tf.TrackNumbers.FirstOrDefault(x => x is TrackNumber.Internal);
                    if (itn is TrackNumber)
                        return $"ITN{itn.Value,3}";
                }

                return "-";
            }
        }
    }
}
