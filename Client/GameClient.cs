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
        const int SCREEN_WIDTH = 1000;
        const int SCREEN_HEIGHT = 700;
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
    
        //Player
        Player _player = new Player();
        Texture2D _playerTexture;
        Texture2D _enemyTexture;

        public GameClient()
        {
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
                        _playerTexture = LoadTexture("Adventure/scarfy.png");
                        _enemyTexture = LoadTexture("Adventure/scarfy.png");

                        _gameScenes = GameScenes.IN_GAME;

                        break;

                    case GameScenes.IN_GAME:

                        BeginDrawing();
                        ClearBackground(RAYWHITE);

                        //Update player in server
                        _gameClient.UpdateClientNetworkObject(_player);

                        if (IsKeyDown(KEY_RIGHT))
                            _player.position_x ++;

                        if (IsKeyDown(KEY_LEFT))
                            _player.position_x --;

                        if (IsKeyDown(KEY_DOWN))
                            _player.position_y ++;

                        if (IsKeyDown(KEY_UP))
                            _player.position_y --;    

                        DrawMap();
                        DrawPlayer();
                        DrawServerImages();

                        EndDrawing();
                        break;
                }       
            }

            EndGame();
        }

        private void DrawServerImages()
        {
            foreach (var netObj in _gameClient.GetServerObjects())
            {
                var serverPlayer = _gameClient.GetClientFromNetworkObject<Player>(netObj);

                var posVec = new Vector2(serverPlayer.position_x, serverPlayer.position_y);

                var playerRec = new Rectangle(0, 0, _enemyTexture.width/6, _enemyTexture.height);

                var resizedRec = new Rectangle(posVec.X, posVec.Y, Math.Abs(playerRec.width), Math.Abs(playerRec.height));

                DrawTexturePro(_enemyTexture, playerRec, resizedRec, new Vector2(resizedRec.width/2, resizedRec.height/2), 0f, WHITE);    
            }
        }

        private void DrawPlayer()
        {
            var posVec = new Vector2(_player.position_x, _player.position_y);

            var playerRec = new Rectangle(0, 0, _playerTexture.width/6, _playerTexture.height);

            var resizedRec = new Rectangle(posVec.X, posVec.Y, Math.Abs(playerRec.width), Math.Abs(playerRec.height));

            DrawTexturePro(_playerTexture, playerRec, resizedRec, new Vector2(resizedRec.width/2, resizedRec.height/2), 0f, WHITE);                
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

            return !notDrawableTypes.Contains( _tiledTileset.tiles.FindLast(x => x.id == (int)tile_id)?.type.ToLowerInvariant() );
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