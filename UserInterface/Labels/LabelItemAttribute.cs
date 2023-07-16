using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_csharp.UserInterface.Labels
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LabelItemAttribute : Attribute
    {
        [Flags]
        public enum TargetClasses
        {
            None = 0,
            TacticalElement = 1,
            TrackFile = 2,
            Everything = 1 & 2,
        }

        public string Name = "New Label Type";
        public TargetClasses AvailableOn { get; set; }

        public LabelItemAttribute(string Name, TargetClasses AvailableTypes)
        {
            this.Name = Name;
            AvailableOn = AvailableTypes;
        }
    }
}
