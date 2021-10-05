using System;

namespace rpg_base_template.Server.TileSystem
{
    public class Map
    {
        private Tile[][] _tiles;

        public Tile[][] get_tiles()
        {
            return _tiles;
        }

        public Tile get_tile_by_position(int x_pos, int y_pos)
        {
            try
            {
                return _tiles[x_pos][y_pos];
            }
            catch (ArgumentException err)
            {
                Console.WriteLine(err);
                return null;
            }
        }
    }

    public class Tile
    {
        private int _spriteId;
        private bool _hasSomethingOnTop;
    }

    public class TileSystem 
    {
        private const int NUMBER_OF_MAPS = 1;
        private const int X_TILES = 25; // Number of tiles in X axis on client
        private const int Y_TILES = 18; // Number of tiles in Y axis on client
        private const int AUX_TILES = 5; // Number of extra tiles to send for client

        private Map[] _world;

        private Tile[][] get_map_tiles_by_point(int map_number, int x_pos, int y_pos)
        {
            Tile[][] _response = new Tile[][] {};
            Tile[][] _map = _world[map_number].get_tiles();

            for (int cursorX=0;cursorX < X_TILES + (2*AUX_TILES); cursorX++)
            {
                for (int cursorY=0; cursorY < Y_TILES + (2*AUX_TILES); cursorY++)
                {

                }
            }
            

            return _response;
        }
    }
}