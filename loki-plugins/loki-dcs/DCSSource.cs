using loki_bms_common;
using loki_bms_common.Database;
using System.Xml.Serialization;

namespace loki_dcs
{
    [XmlInclude(typeof(DCSSource))]
    public class DCSSource : LokiDataSource
    {
        public override SerializedDataSource GetSerializable()
        {
            var sds = base.GetSerializable();
            sds.Extradata = new string[] { $"Extra Data Test from DCSSource {Name}" };

            return sds;
        }

        public override void LoadSerializable(SerializedDataSource sds)
        {
            base.LoadSerializable(sds);

            SourceInfo = $"{GetType().Name} at {Address}:{Port}";
        }

        public override void Activate()
        {

        }

        public override bool CheckAlive()
        {
            return false;
        }

        public override void Deactivate()
        {

        }

        public override TrackDatum[] GetFreshData()
        {
            throw new NotImplementedException();
        }
    }
}
