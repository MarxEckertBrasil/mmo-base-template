using NihilNetwork.Client;
using Newtonsoft.Json;

using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Color;
using static Raylib_cs.KeyboardKey;
using System.IO;
using rpg_base_template.Client.System;
using System;
using System.Linq;
using NihilNetwork;
using System.Collections.Generic;

namespace rpg_base_template.Client
{
    public enum GameScenes
    {
        MAIN_MENU = 0,
        LOBBY = 2,
        LOADING_GAME = 3,
        IN_GAME = 4
    }

    public class GameClient
    {
        NihilNetworkClient _gameClient;

        const int IMAGE_SCALE = 4;
        const int SCREEN_WIDTH = 1920;
        const int SCREEN_HEIGHT = 1080;
        const int NUM_FRAMES = 3;

        Texture2D _systemButton;
        Rectangle _btnBounds;
        Rectangle _sourceRec;
        Sound _fxButton;

        int _frameHeight;
        Vector2 _mousePoint = new Vector2(0.0f, 0.0f);

        GameScenes _gameScenes = GameScenes.MAIN_MENU;

        //TILED.
        const string TILED_PATH = "Adventure/";
        const string MAP_NAME = "map.json";
        Texture2D _tiledMapTexture;
        TiledMap _tiledMap = new TiledMap();
        TiledTileset _tiledTileset = new TiledTileset();
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG   = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG   = 0x20000000;
        List<Rectangle> _collideTiles = new List<Rectangle>();

        //Player
        Player _player = new Player();
        Texture2D _playerTexture;
        Texture2D _enemyTexture;

        public GameClient()
        {
            //Bordless mode
            SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            SetWindowState(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
            SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);

            InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "rpg-base-template");
            InitAudioDevice();
                   
            _gameClient = new NihilNetworkClient();

            _systemButton = LoadTexture("System/Images/Button1.png");

            _frameHeight = _systemButton.height/NUM_FRAMES;
            _sourceRec = new Rectangle(0, 0, _systemButton.width, _frameHeight);
            _btnBounds = new Rectangle(SCREEN_WIDTH/2 - _systemButton.width/2, SCREEN_HEIGHT/2 - _systemButton.height/NUM_FRAMES/2, _systemButton.width, _frameHeight);

            _fxButton = LoadSound("System/Audio/buttonfx.wav");
            
            SetTargetFPS(60);
        }

        public void GameLoop()
        {
            int btnState = 0;

            while (!WindowShouldClose())
            {
                switch (_gameScenes)
                {
                    case GameScenes.MAIN_MENU:
                        _mousePoint = GetMousePosition();
                        var btnAction = false;

                        if (CheckCollisionPointRec(_mousePoint, _btnBounds))
                        {
                            if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                                btnState = 2;
                            else
                                btnState = 1;

                            if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                                btnAction = true;
                        }
                        else
                            btnState = 0;

                        if (btnAction)
                        {
                            PlaySound(_fxButton);

                            while (IsSoundPlaying(_fxButton))
                            {

                            }
                       
                            _gameClient.Connect("127.0.0.1", 1120, this, true, true);

                            this.RegisterNetworkFunctions(_gameClient);  

                            _gameScenes = GameScenes.LOADING_GAME;
                        }

                        _sourceRec.y = btnState * _frameHeight;

                        BeginDrawing();
                        ClearBackground(RAYWHITE);

                        DrawTextureRec(_systemButton, _sourceRec, new Vector2(_btnBounds.x, _btnBounds.y), WHITE);

                        EndDrawing();
                        break;

                    case GameScenes.LOBBY:
                        break;

                    case GameScenes.LOADING_GAME:

                        ConfigureTiled();

                        //Add player texture
                        _playerTexture = LoadTexture("Adventure/player.png");
                        _enemyTexture = LoadTexture("Adventure/scarfy.png");

                        _gameScenes = GameScenes.IN_GAME;

                        break;

                    case GameScenes.IN_GAME:

                        BeginDrawing();
                        ClearBackground(RAYWHITE);

                        //Update player in server
                        _gameClient.UpdateClientNetworkObject(_player);
                      
                        DrawMap();
                        var playerRec = DrawPlayer();
                        DrawServerImages();

                        var moveVec = new Vector2(0,0);

                        if (IsKeyDown(KEY_RIGHT))
                            moveVec.X ++;
                        if (IsKeyDown(KEY_LEFT))
                            moveVec.X --;
                        if (IsKeyDown(KEY_DOWN))
                            moveVec.Y ++;
                        if (IsKeyDown(KEY_UP))
                            moveVec.Y --;   

                        _player.position += moveVec;

                        var collisionAreas = GetCollisionAreas(playerRec);
                        _player.position += GetDirection(playerRec, collisionAreas);

                        EndDrawing();

                        break;
                }       
            }

            EndGame();
        }
        private double GetDistance(Vector2 pos1, Vector2 pos2)
        {
            return Math.Sqrt( Math.Sqrt( pos2.X - pos1.X) + Math.Sqrt( pos2.Y - pos1.Y) );
        }

        private Vector2 GetDirection(Rectangle playerRec, List<Rectangle> collisionAreas)
        {
            var direction = new Vector2(0, 0);
            const float MARGIN = 5;

            foreach (var rec in collisionAreas)
            {
                if (rec.height >= MARGIN)
                {
                    if (playerRec.x == rec.x)
                        direction.X += rec.width;
                    else
                        direction.X += -rec.width;
                }

                if (rec.width >= MARGIN)
                {
                    if (playerRec.y == rec.y)
                        direction.Y += rec.height;
                    else
                        direction.Y += -rec.height;
                }
            }

            return direction;
        }

        private List<Rectangle> GetCollisionAreas(Rectangle playerRec)
        {
            var collisionAreas = new List<Rectangle>();
            foreach (var rec in _collideTiles)
            {
                if (CheckCollisionRecs(playerRec, rec))
                    collisionAreas.Add(GetCollisionRec(playerRec, rec));
            }

           return collisionAreas;
        }

        private void DrawServerImages()
        {
            foreach (var netObj in _gameClient.GetServerObjects())
            {
                var serverPlayer = _gameClient.GetClientFromNetworkObject<Player>(netObj);

                var posVec = new Vector2(serverPlayer.position.X, serverPlayer.position.Y);

                var playerRec = new Rectangle(0, 0, _enemyTexture.width/6, _enemyTexture.height);

                var resizedRec = new Rectangle(posVec.X, posVec.Y, Math.Abs(playerRec.width), Math.Abs(playerRec.height));

                DrawTexturePro(_enemyTexture, playerRec, resizedRec, new Vector2(resizedRec.width/2, resizedRec.height/2), 0f, WHITE);    
            }
        }

        private Rectangle DrawPlayer()
        {
            var posVec = new Vector2(_player.position.X, _player.position.Y);

            var playerRec = new Rectangle(0, 0, _playerTexture.width, _playerTexture.height);

            var resizedRec = new Rectangle(posVec.X, posVec.Y, Math.Abs(playerRec.width * IMAGE_SCALE), Math.Abs(playerRec.height * IMAGE_SCALE));

            DrawTexturePro(_playerTexture, playerRec, resizedRec, new Vector2(resizedRec.width/2, resizedRec.height/2), 0f, WHITE); 

            return resizedRec;               
        }

        private void DrawMap()
        {          
            foreach (var layer in _tiledMap.layers)
            {
                int x_pos = 0;
                int y_pos = 0;
          
                foreach (var tile in layer.data)
                {   
                    var tile_id = tile;
                    tile_id &= ~(FLIPPED_HORIZONTALLY_FLAG |
                                 FLIPPED_VERTICALLY_FLAG   |
                                 FLIPPED_DIAGONALLY_FLAG   );


                    if (tile_id > 0 && IsTileDrawable(tile_id))
                    {        
                        var rotate = 0f;
                        var posVec = new Vector2(x_pos*_tiledMap.tilewidth, y_pos*_tiledMap.tileheight);
                        
                        var tileRec = GetTileRecById(tile_id);
                        
                        if ((tile & FLIPPED_DIAGONALLY_FLAG) > 0)
                        {
                            rotate = -90f;
                            tileRec.height = -tileRec.height;
                        }  

                        if ((tile & FLIPPED_HORIZONTALLY_FLAG) > 0)
                        {
                            tileRec.width = -tileRec.width;
                        }

                        if ((tile & FLIPPED_VERTICALLY_FLAG) > 0)
                        {
                            tileRec.height = -tileRec.height;
                        }
                                                
                        var resizedTileRec = new Rectangle(posVec.X*IMAGE_SCALE, posVec.Y*IMAGE_SCALE, Math.Abs(tileRec.width)*IMAGE_SCALE, Math.Abs(tileRec.height)*IMAGE_SCALE);
                        DrawTexturePro(_tiledMapTexture, tileRec, resizedTileRec, new Vector2(resizedTileRec.width/2, resizedTileRec.height/2), rotate, WHITE);
                    
                        DrawRectangleLines((int)resizedTileRec.x - (int)resizedTileRec.width/2, (int)resizedTileRec.y - (int)resizedTileRec.height/2, (int)resizedTileRec.width, (int)resizedTileRec.height, RED);
                    }

                    x_pos++;
                    if (x_pos >= layer.width)
                    {
                        x_pos = 0;
                        y_pos++;
                    }
                }
            }
        }

        private bool IsTileDrawable(uint tile_id)
        {
            var notDrawableTypes = new string[] { "monster", "chest"};

            return !notDrawableTypes.Contains( _tiledTileset.tiles.FindLast(x => x.id == ((int)tile_id - 1))?.type.ToLowerInvariant() );
        }

        private bool IsTileCollide(uint tile_id)
        {
            var collideTypes = new string[] { "monster", "chest", "wall" };

            return collideTypes.Contains( _tiledTileset.tiles.FindLast(x => x.id == ((int)tile_id - 1))?.type.ToLowerInvariant() );
        }

        private Rectangle GetTileRecById(uint tile)
        {
            var x_pos = (tile % _tiledTileset.x_tiles - 1) * _tiledTileset.tilewidth;
            var y_pos = (int)(Math.Ceiling((decimal)tile / _tiledTileset.y_tiles) - 1) * _tiledTileset.tileheight;
            
            var rec = new Rectangle(x_pos, y_pos, _tiledMap.tilewidth, _tiledMap.tileheight);

            return rec;
        }

        public void ConfigureTiled()
        {
            using StreamReader reader = new StreamReader(TILED_PATH + MAP_NAME);
            
            string json = reader.ReadToEnd();
            _tiledMap = JsonConvert.DeserializeObject<TiledMap>(json);

            //Change the logic in future for multiples tilesets
            foreach (var tileset in _tiledMap.tilesets)
            {
                using StreamReader tilesetReader = new StreamReader(TILED_PATH + (tileset.source).Remove(0, 3).Remove(tileset.source.Length - 7) + ".json");
                json = tilesetReader.ReadToEnd();
                _tiledTileset = JsonConvert.DeserializeObject<TiledTileset>(json);
                _tiledTileset.x_tiles = (int)(_tiledTileset.imagewidth / _tiledTileset.tilewidth);
                _tiledTileset.y_tiles = (int)(_tiledTileset.imageheight / _tiledTileset.tileheight);
            }
            
            _tiledMapTexture = LoadTexture(TILED_PATH + (_tiledTileset.image).Remove(0, 3));  

            //Load Collides
            var collideTiles = new List<CollisionTile>();
            foreach (var layer in _tiledMap.layers)
            {
                int x_pos = 0;
                int y_pos = 0;
          
                foreach (var tile in layer.data)
                {   
                    var tile_id = tile;
                    tile_id &= ~(FLIPPED_HORIZONTALLY_FLAG |
                                 FLIPPED_VERTICALLY_FLAG   |
                                 FLIPPED_DIAGONALLY_FLAG   );


                    if (tile_id > 0 && IsTileDrawable(tile_id))
                    {        
                        var posVec = new Vector2(x_pos*_tiledMap.tilewidth, y_pos*_tiledMap.tileheight);
                        
                        var tileRec = GetTileRecById(tile_id);
                                     
                        var resizedTileRec = new Rectangle(posVec.X*IMAGE_SCALE, posVec.Y*IMAGE_SCALE, Math.Abs(tileRec.width)*IMAGE_SCALE, Math.Abs(tileRec.height)*IMAGE_SCALE);

                        if (IsTileCollide(tile_id))
                            collideTiles.Add(new CollisionTile() { Rec = resizedTileRec, Used = false });
                    }

                    x_pos++;
                    if (x_pos >= layer.width)
                    {
                        x_pos = 0;
                        y_pos++;
                    }
                }
            }

            _collideTiles.Clear();
            //Configure collisions more precise
            foreach (var tile1 in collideTiles)
            {
                if (tile1.Used)
                    continue;

                tile1.Used = true;
                    
                var newCollisionRec = tile1.Rec;
                
                foreach (var tile2 in collideTiles)
                {
                    if (tile2.Used)
                        continue;

                    if (GetDistance(new Vector2(tile2.Rec.x + tile2.Rec.width, tile2.Rec.y),
                                    new Vector2(newCollisionRec.x, newCollisionRec.y)) == 0)
                    {
                        newCollisionRec.x = tile2.Rec.x;
                        
                        newCollisionRec.width += tile2.Rec.width;

                        tile2.Used = true;
                    }
                    else if (GetDistance(new Vector2(tile2.Rec.x, tile2.Rec.y + tile2.Rec.height), 
                                         new Vector2(newCollisionRec.x, newCollisionRec.y)) == 0)
                    {
                        newCollisionRec.y = tile2.Rec.y;
                        
                        newCollisionRec.height += tile2.Rec.height;

                        tile2.Used = true;
                    }
                    else if (GetDistance(new Vector2(newCollisionRec.x + newCollisionRec.width, newCollisionRec.y), 
                                         new Vector2(tile2.Rec.x, tile2.Rec.y)) == 0)
                    {
                        newCollisionRec.width += tile2.Rec.width;

                        tile2.Used = true;
                    }
                    else if (GetDistance(new Vector2(newCollisionRec.x, newCollisionRec.y + newCollisionRec.height), 
                                         new Vector2(tile2.Rec.x, tile2.Rec.y)) == 0)
                    {
                        newCollisionRec.height += tile2.Rec.height;

                        tile2.Used = true;
                    }
                }

                _collideTiles.Add(newCollisionRec);
            }

        }

        public void EndGame()
        {
            UnloadTexture(_systemButton);
            if (_tiledMapTexture.format != 0)
                UnloadTexture(_tiledMapTexture);
                 
            UnloadSound(_fxButton);
    
            _gameClient.Disconect();
            CloseAudioDevice();
            CloseWindow();
        }
        public void RegisterNetworkFunctions(NihilNetworkClient gameclient)
        {
        } 
    }
}