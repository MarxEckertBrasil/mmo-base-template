using System.Numerics;
using NihilNetwork;

namespace rpg_base_template.Client.System
{
    public class GameObject
    {
        [NihilNetworkObjectId]
        public int ObjectId = -1;

        [NihilNetworkMonitored]
        public Vector2 Position = new Vector2(0, 0);

        [NihilNetworkMonitored]
        public Vector2 Size = new Vector2(0,0);
        
        [NihilNetworkMonitored]
        public float VisionRange;

        [NihilNetworkMonitored]
        public int MapId = -1; 

        [NihilNetworkMonitored]
        public uint TileId;

        [NihilNetworkMonitored]
        public bool Visible = true;
    }
}