using System.Numerics;
using NihilNetwork;

namespace rpg_base_template.Client.System
{
    public class Player
    {
        [NihilNetworkObjectId]
        public int objectId = -1;

        [NihilNetworkMonitored]
        public float position_x = 128f;

        [NihilNetworkMonitored]
        public float position_y = 128f;
    }
}