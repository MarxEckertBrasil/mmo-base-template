using System.Numerics;
using NihilNetwork;

namespace rpg_base_template.Client.System
{
    public class Player
    {
        [NihilNetworkObjectId]
        public int objectId = -1;

        [NihilNetworkMonitored]
        public Vector2 position = new Vector2(256f, 256f);
    }
}