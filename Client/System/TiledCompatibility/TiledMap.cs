using System.Collections.Generic;
using Raylib_cs;

namespace rpg_base_template.Client.System
{
    public class TiledMap
    {
        public int compressionlevel { get; set; }
        public int height { get; set; }
        public bool infinite { get; set; }
        public List<Layer> layers { get; set; }
        public int nextlayerid { get; set; }
        public int nextobjectid { get; set; }
        public string orientation { get; set; }
        public string renderorder { get; set; }
        public string tiledversion { get; set; }
        public int tileheight { get; set; }
        public List<Tileset> tilesets { get; set; }
        public int tilewidth { get; set; }
        public string type { get; set; }
        public string version { get; set; }
        public int width { get; set; }


        internal int MapId {get; set; }
        internal List<Rectangle> CollideTiles = new List<Rectangle>();
        internal List<(int Firstgid, TiledTileset Tileset)> TiledTilesets = new List<(int Firstgid, TiledTileset Tileset)>();
        internal List<(int Firstgid, Texture2D Texture)> TiledMapTextures = new List<(int Firstgid, Texture2D Texture)>();
        internal List<(uint DoorId, string DoorPath, Rectangle Rec)> MapDoors = new List<(uint DoorId, string DoorPath, Rectangle Rec)>();
        internal List<GameObject> GameObjects = new List<GameObject>();
    }
    public class Layer
    {
        public List<uint> data { get; set; }
        public int height { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int opacity { get; set; }
        public string type { get; set; }
        public bool visible { get; set; }
        public int width { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public string draworder { get; set; }
        public List<TileObject> objects { get; set; }
    }

    public class Tileset
    {
        public int firstgid { get; set; }
        public string source { get; set; }
    }

    public class TileObject
    {
        public double height { get; set; }
        public uint id { get; set; }
        public string name { get; set; }
        public int rotation { get; set; }
        public string type { get; set; }
        public bool visible { get; set; }
        public double width { get; set; }
        public double x { get; set; }
        public double y { get; set; }
    }
}