using System.Numerics;
using NihilNetwork;
using Raylib_cs;

namespace rpg_base_template.Client.System
{
    public class Player
    {
        [NihilNetworkObjectId]
        public int ObjectId = -1;

        [NihilNetworkMonitored]
        public Vector2 Position = new Vector2(0, 0);

        [NihilNetworkMonitored]
        public float VisionRange;

        [NihilNetworkMonitored]
        public int MapId = -1; 
        [NihilNetworkMonitored]
        public uint TileId;
    }
}