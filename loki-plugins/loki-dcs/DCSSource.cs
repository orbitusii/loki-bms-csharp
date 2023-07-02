using loki_bms_common;
using loki_bms_common.Database;

namespace loki_dcs
{
    public class DCSSource : LokiDataSource
    {


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
