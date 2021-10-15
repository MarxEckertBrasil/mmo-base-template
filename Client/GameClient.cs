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
using System.Runtime.InteropServices;
using NihilNetwork.Utils;
using System.Net;
using NihilNetwork.Server;

namespace rpg_base_template.Client
{
    public enum GameScenes
    {
        MAIN_MENU = 0,
        LOBBY = 2,
        LOADING_GAME = 3,
        SELECT_CHARACTER = 4,
        IN_GAME = 5
    }

    public class GameClient
    {
        //Network
        NihilNetworkClient _gameClient;
        NihilNetworkServer _gameServer;
        bool _isServer;

        const int IMAGE_SCALE = 4;
        const int SCREEN_WIDTH = 800;
        const int SCREEN_HEIGHT = 800;
        const int NUM_FRAMES = 3;
        Color BACKGROUND_COLOR = BLACK;

        Texture2D _joinButton;
        Texture2D _hostButton;
        
        Rectangle _joinBtnBnds;
        Rectangle _joinBtnRec;

        Rectangle _hostBtnBnds;
        Rectangle _hostBtnRec;
        Sound _fxButton;

        int _frameHeight;
        int _frameHeight2;
        Vector2 _mousePoint = new Vector2(0.0f, 0.0f);

        GameScenes _gameScenes = GameScenes.MAIN_MENU;

        //TILED.
        const string TILED_PATH = "Adventure/";
        const string MAP_NAME = "map.json";
        List<TiledMap> _tiledMaps = new List<TiledMap>();
        TiledMap _currentTiledMap = new TiledMap();
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG   = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG   = 0x20000000;
        
        //Player
        Player _player = new Player();
        Camera2D _camera = new Camera2D();


        //Effects
        Shader _shader = new Shader();
        public GameClient()
        {
            //Bordless mode
            //SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            //SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            //SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            //SetWindowState(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
            //SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);

            InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "rpg-base-template");
            InitAudioDevice();
                   
            _gameClient = new NihilNetworkClient();
            _gameServer = new NihilNetworkServer();

            //Button1
            _joinButton = LoadTexture("System/Images/Button1.png");

            _frameHeight = _joinButton.height/NUM_FRAMES;
            _joinBtnRec = new Rectangle(0, 0, _joinButton.width, _frameHeight);
            _joinBtnBnds = new Rectangle(SCREEN_WIDTH/2 - _joinButton.width/2, SCREEN_HEIGHT/2 - _joinButton.height/NUM_FRAMES/2, _joinButton.width, _frameHeight);

            _fxButton = LoadSound("System/Audio/buttonfx.wav");
            
            //Button2
            _hostButton = LoadTexture("System/Images/Button1.png");

            _frameHeight2 = _hostButton.height/NUM_FRAMES;
            _hostBtnRec = new Rectangle(0, 0, _hostButton.width, _frameHeight2);
            _hostBtnBnds = new Rectangle(SCREEN_WIDTH/2 - _hostButton.width/2, SCREEN_HEIGHT/2 - _hostButton.height/NUM_FRAMES/2 + _joinBtnBnds.height, _hostButton.width, _frameHeight2);


            SetTargetFPS(60);
        }

        public void GameLoop()
        {
            int joinBtnState = 0;
            int hostBtnState = 0;
            var mouseClick = true;

            var inputIp = string.Empty;
            Rectangle inputIpBox = new Rectangle(SCREEN_WIDTH/2 - 100, 180, 225, 50);            
            var inputIpBoxSelected = false;

            var targetIp = "127.0.0.1";
            
            var charCount = 0;
            var mousePos = new Vector2(0,0);
            while (!WindowShouldClose())
            {
                switch (_gameScenes)
                {
                    case GameScenes.MAIN_MENU:
                        _mousePoint = GetMousePosition();
                        var btnAction = false;

                        if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                            mouseClick = true;
                        
                        if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                            mouseClick = false;

                        //Input text logic   
                        if (CheckCollisionPointRec(_mousePoint, inputIpBox))
                        {
                            if (mouseClick)
                            {
                                inputIpBoxSelected = true;
                            }
                        }
                        else if (mouseClick)
                        {
                            inputIpBoxSelected = false;
                        }
                       
                        if (inputIpBoxSelected)
                        {
                            int key = GetCharPressed();

                            while (key > 0)
                            {
                                if ((key >= 32) && (key <= 125) && (charCount < 15))
                                {
                                    targetIp += (char)key;
                                    charCount++;
                                }

                                key = GetCharPressed();                            
                            }

                            if (IsKeyPressed(KEY_BACKSPACE))
                            {
                                charCount--;
                                if (charCount < 0) charCount = 0;
                                targetIp = targetIp.Substring(0, charCount);
                            }
                        }
                        
                        if (CheckCollisionPointRec(_mousePoint, _joinBtnBnds))
                        {
                            if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                                joinBtnState = 2;
                            else
                                joinBtnState = 1;

                            if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                                btnAction = true;
                        }
                        else
                            joinBtnState = 0;

                        if (CheckCollisionPointRec(_mousePoint, _hostBtnBnds))
                        {
                            if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                                hostBtnState = 2;
                            else
                                hostBtnState = 1;

                            if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                            {
                                _isServer = true;
                                btnAction = true;
                            }
                        }
                        else
                            hostBtnState = 0;

                        if (btnAction)
                        {
                            if (_isServer)
                                targetIp = "127.0.0.1";

                            var validIp = IPAddress.TryParse(targetIp, out IPAddress addr);

                            if (validIp)
                            {
                                inputIp = targetIp;

                                PlaySound(_fxButton);

                                while (IsSoundPlaying(_fxButton))
                                {}

                                _gameScenes = GameScenes.LOADING_GAME;
                            }
                        }

                        _joinBtnRec.y = joinBtnState * _frameHeight;
                        _hostBtnRec.y = hostBtnState * _frameHeight2;

                        BeginDrawing();
                        ClearBackground(BACKGROUND_COLOR);

                        DrawRectangleRec(inputIpBox, GRAY);
                        DrawTextureRec(_joinButton, _joinBtnRec, new Vector2(_joinBtnBnds.x, _joinBtnBnds.y), WHITE);
                        DrawTextureRec(_hostButton, _hostBtnRec, new Vector2(_hostBtnBnds.x, _hostBtnBnds.y), WHITE);

                        DrawText(targetIp, (int)inputIpBox.x, (int)inputIpBox.y, 40, BLACK);
                        if (inputIpBoxSelected)
                            DrawRectangleLines((int)inputIpBox.x, (int)inputIpBox.y, (int)inputIpBox.width, (int)inputIpBox.height, YELLOW);

                        EndDrawing();
                        break;

                    case GameScenes.LOBBY:
                        break;

                    case GameScenes.LOADING_GAME:

                        if (_isServer)
                            _gameServer.StartServer(1120, this, 10, true, true);

                         _gameClient.Connect(inputIp, 1120, this, 5, true, true);

                        //Configure first map
                        ImportTiledMap(MAP_NAME);
                        _currentTiledMap = _tiledMaps[0];

                        //Load shader
                        _shader = LoadShader("System/Shaders/gray.vs", "System/Shaders/gray.fs");

                        //Load camera
                        _camera.offset = new Vector2 (){ X = SCREEN_WIDTH/2.0f, Y = SCREEN_HEIGHT/2.0f };
                        _camera.rotation = 0.0f;
                        _camera.zoom = 1.0f;                     

                        _gameScenes = GameScenes.SELECT_CHARACTER;

                        break;

                    case GameScenes.SELECT_CHARACTER:
                        if (_isServer)
                        {
                            _gameScenes = GameScenes.IN_GAME;
                            continue;
                        }

                        mousePos = GetScreenToWorld2D(GetMousePosition(), _camera);
                        
                        BeginDrawing();
                        BeginMode2D(_camera);
                        ClearBackground(BACKGROUND_COLOR);

                        DrawMap(false);
                        var charactersToSelect = GetSpecificTileType("Player");
                        var indexMiddleCharacter = (int)Math.Floor((decimal)(charactersToSelect.Count() / 2) - 1 );
                        _camera.target = new Vector2 (){ X = charactersToSelect[indexMiddleCharacter].ResizedRec.x - charactersToSelect[indexMiddleCharacter].ResizedRec.width + 20.0f, 
                                                        Y = charactersToSelect[indexMiddleCharacter].ResizedRec.y - charactersToSelect[indexMiddleCharacter].ResizedRec.height + 20.0f };
                        
                        foreach (var character in charactersToSelect)
                        {
                            if (CheckCollisionPointRec(mousePos, character.ResizedRec))
                            {
                                DrawRectangleLines((int)(character.ResizedRec.x), (int)(character.ResizedRec.y), 
                                                (int) character.ResizedRec.width, (int) character.ResizedRec.height, YELLOW);

                                if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                                {
                                    _player.Position = new Vector2(character.ResizedRec.x + character.ResizedRec.width/2, character.ResizedRec.y + character.ResizedRec.height/2);
                                    _player.MapId = character.MapId;
                                    _player.TileId = character.TileId;
                                    break;
                                }
                            } 
                        }

                        EndMode2D();

                        DrawText("CHOOSE A CHARACTER TO START THE ADVENTURE", SCREEN_WIDTH/2 - 220, (int)_camera.target.Y - 20, 20, WHITE);
                        
                        EndDrawing();

                        if (_player.TileId != 0 && _player.MapId != -1)
                        {
                            if (_player.VisionRange == 0)
                            {
                                _player.VisionRange = 2 * _tiledMaps[_player.MapId].tilewidth * IMAGE_SCALE;
                            }

                            _gameScenes = GameScenes.IN_GAME;
                        }
                        

                        break;

                    case GameScenes.IN_GAME:
                                                     
                        BeginDrawing();                  
                        ClearBackground(BACKGROUND_COLOR);

                        BeginMode2D(_camera);
                        
                        _camera.target = new Vector2 (){ X = _player.Position.X + 20.0f, Y = _player.Position.Y + 20.0f };

                        //Update player in server
                        if (_isServer)
                            _gameClient.UpdateClientNetworkObject(_player); 
                        else
                        {
                           var ranges = new Dictionary<string, NihilNetworkRange>();
                           ranges.Add("Position", _gameClient.GetNetworkRange(NihilNetworkOperations.LESS_EQUAL, _player.VisionRange, "Position", true));
                            
                           _gameClient.UpdateClientNetworkObject(_player, ranges);                     
                        }
                        
                        BeginShaderMode(_shader);
                        DrawMap();
                        EndShaderMode();

                        
                        DrawServerImages();
                        
                        if (_isServer)
                        {
                        
                            if (IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                            {
                                mousePos = GetScreenToWorld2D(GetMousePosition(), _camera);                             
                            }
                            else if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                            {
                                var releasedMOusePos = GetScreenToWorld2D(GetMousePosition(), _camera);
                                _player.Position += (mousePos - releasedMOusePos ) * 50;
                            }
                            else
                                mousePos = new Vector2(0,0);
                        }
                        else
                        {
                            var playerRec = DrawPlayer();
                            var moveVec = new Vector2(0,0);

                            if (IsKeyDown(KEY_RIGHT))
                                moveVec.X ++;
                            if (IsKeyDown(KEY_LEFT))
                                moveVec.X --;
                            if (IsKeyDown(KEY_DOWN))
                                moveVec.Y ++;
                            if (IsKeyDown(KEY_UP))
                                moveVec.Y --;   

                            _player.Position += moveVec;

                            var collisionAreas = GetCollisionAreas(playerRec);
                            _player.Position += GetDirection(playerRec, collisionAreas);
                        }
                            
                        EndMode2D();             
                        EndDrawing();

                        //Uncomment for use only with a server
                        if (_isServer)
                        {
                            if (!_gameServer.IsServerRunning() || !_gameClient.Connection.Connected)
                            {
                                _isServer = false;
                                _gameScenes = GameScenes.MAIN_MENU;

                                _gameClient.Disconnect();
                                _gameServer.StopServer();
                            }
                        }
                        else if (!_gameClient.Connection.Connected)
                        {
                            _gameClient.Disconnect();
                            _gameScenes = GameScenes.MAIN_MENU;
                        }

                        break;
                }       
            }

            EndGame();
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
            foreach (var rec in _currentTiledMap.CollideTiles)
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
                DrawEntity(serverPlayer.TileId, serverPlayer.MapId, serverPlayer.Position);  
            }
        }

        private Rectangle DrawPlayer()
        {
            var posVec = new Vector2(_player.Position.X, _player.Position.Y);
            var tiledMap = _tiledMaps[_player.MapId];

            var tileRec = GetTileRecById(_player.TileId, tiledMap);
            
            var resizedRec = new Rectangle(posVec.X, posVec.Y, Math.Abs(tileRec.width * IMAGE_SCALE), Math.Abs(tileRec.height * IMAGE_SCALE));

            DrawTexturePro(tiledMap.TiledMapTextures.LastOrDefault(x => _player.TileId >= x.Firstgid).Texture, tileRec, resizedRec, new Vector2(resizedRec.width/2, resizedRec.height/2), 0f, WHITE); 

            return resizedRec;               
        }

        private void DrawEntity(uint tileId, int mapId, Vector2 position)
        {
            var tiledMap = _tiledMaps[mapId];
            var tileRec = GetTileRecById(tileId, tiledMap);
            var resizedTileRec = new Rectangle(position.X, position.Y, Math.Abs(tileRec.width * IMAGE_SCALE), Math.Abs(tileRec.height)*IMAGE_SCALE);

            DrawTexturePro(tiledMap.TiledMapTextures.LastOrDefault(x => tileId >= x.Firstgid).Texture, tileRec, resizedTileRec, new Vector2(resizedTileRec.width/2, resizedTileRec.height/2), 0f, WHITE);                          
        }

        private void DrawMap(bool activeRender = true)
        {          
            foreach (var layer in _currentTiledMap.layers)
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
                        var posVec = new Vector2(x_pos*_currentTiledMap.tilewidth, y_pos*_currentTiledMap.tileheight);
                        
                        var tileRec = GetTileRecById(tile_id, _currentTiledMap);
                        
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

                        var inVision = NihilNetworkUtils.Get2dDistanceBetween(new Vector2(resizedTileRec.x, resizedTileRec.y), _player.Position) <= _player.VisionRange;
                        
                        if (inVision && activeRender)
                            EndShaderMode();                     
                        
                        DrawTexturePro(_currentTiledMap.TiledMapTextures.LastOrDefault(x => tile_id >= x.Firstgid).Texture, tileRec, resizedTileRec, new Vector2(resizedTileRec.width/2, resizedTileRec.height/2), rotate, WHITE);                    
                            
                        if (inVision && activeRender)
                            BeginShaderMode(_shader);

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

        private List<(int MapId, uint TileId, Rectangle ResizedRec)> GetSpecificTileType(string tileType)
        {          
            var characters = new List<(int MapId, uint TileId,  Rectangle ResizedRec)>();
            
            foreach (var layer in _currentTiledMap.layers)
            {
                int x_pos = 0;
                int y_pos = 0;
          
                foreach (var tile in layer.data)
                {   
                    var tile_id = tile;
                    tile_id &= ~(FLIPPED_HORIZONTALLY_FLAG |
                                 FLIPPED_VERTICALLY_FLAG   |
                                 FLIPPED_DIAGONALLY_FLAG   );

                    var tileTileset = _currentTiledMap.TiledTilesets.LastOrDefault(x => tile_id >= x.Firstgid);
                    var tileIdType = tileTileset.Tileset?.tiles.FindLast(x => x.id == ((int)tile_id - tileTileset.Firstgid))?.type.ToLowerInvariant();
                    if (tile_id > 0 && tileType.ToLowerInvariant() == tileIdType)
                    {        
                        var rotate = 0f;
                        var posVec = new Vector2(x_pos*_currentTiledMap.tilewidth, y_pos*_currentTiledMap.tileheight);
                        
                        var tileRec = GetTileRecById(tile_id, _currentTiledMap);
                        
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
                        DrawTexturePro(_currentTiledMap.TiledMapTextures.LastOrDefault(x => tile_id >= x.Firstgid).Texture, tileRec, resizedTileRec, new Vector2(resizedTileRec.width/2, resizedTileRec.height/2), rotate, WHITE);                    
                        
                        characters.Add((MapId: _currentTiledMap.MapId, 
                                        TileId: tile_id, 
                                        ResizedRec: new Rectangle(resizedTileRec.x  - (int)resizedTileRec.width/2, resizedTileRec.y - (int)resizedTileRec.height/2, resizedTileRec.width, resizedTileRec.height)));
                    }

                    x_pos++;
                    if (x_pos >= layer.width)
                    {
                        x_pos = 0;
                        y_pos++;
                    }
                }
            }

            return characters;
        }
        private bool IsTileDrawable(uint tile_id)
        {
            var notDrawableTypes = new string[] { "monster", "chest", "player" };

            var tileTileset = _currentTiledMap.TiledTilesets.LastOrDefault(x => tile_id >= x.Firstgid);
            return !notDrawableTypes.Contains( tileTileset.Tileset.tiles.FindLast(x => x.id == ((int)tile_id - tileTileset.Firstgid))?.type.ToLowerInvariant() );
        }

        private bool IsTileCollide(uint tileId, TiledMap tiledMap)
        {
            var collideTypes = new string[] { "monster", "chest", "wall" };

            var tileTileset = tiledMap.TiledTilesets.LastOrDefault(x => tileId >= x.Firstgid);
            return collideTypes.Contains( tileTileset.Tileset.tiles.FindLast(x => x.id == ((int)tileId - tileTileset.Firstgid))?.type.ToLowerInvariant() );
        }

        private Rectangle GetTileRecById(uint tile, TiledMap tiledMap)
        {
            var tileTileset = tiledMap.TiledTilesets.LastOrDefault(x => tile >= x.Firstgid);

            var x_pos = ((tile - tileTileset.Firstgid) % tileTileset.Tileset.x_tiles) * tileTileset.Tileset.tilewidth + tileTileset.Tileset.margin;
            var y_pos = (int)(Math.Floor((decimal)((tile - tileTileset.Firstgid) / tileTileset.Tileset.x_tiles)) * tileTileset.Tileset.tileheight + tileTileset.Tileset.margin);
             
            var rec = new Rectangle(x_pos, y_pos, tiledMap.tilewidth, tiledMap.tileheight);

            return rec;
        }

        public void ImportTiledMap(string mapName)
        {
            using StreamReader reader = new StreamReader(TILED_PATH + mapName);
            
            string json = reader.ReadToEnd();
            var tiledMap = JsonConvert.DeserializeObject<TiledMap>(json);
            tiledMap.MapId = _tiledMaps.Count();

            _tiledMaps.Add(tiledMap);

            //Change the logic in future for multiples tilesets
            foreach (var tileset in tiledMap.tilesets)
            {
                using StreamReader tilesetReader = new StreamReader(TILED_PATH + (tileset.source).Remove(0, 3).Remove(tileset.source.Length - 7) + ".json");
                json = tilesetReader.ReadToEnd();
                var tiledTileset = JsonConvert.DeserializeObject<TiledTileset>(json);

                if (tiledTileset != null)
                {
                    tiledTileset.x_tiles = (int)((tiledTileset.imagewidth - tiledTileset.margin*2) / tiledTileset.tilewidth);

                    tiledMap.TiledTilesets.Add((Firstgid: tileset.firstgid, Tileset: tiledTileset));             
                }
            }
            
            foreach (var tileTileset in tiledMap.TiledTilesets)
            {
                tiledMap.TiledMapTextures.Add((Firstgid: tileTileset.Firstgid, Texture: LoadTexture(TILED_PATH + (tileTileset.Tileset.image).Remove(0, 3))));  

            }

            //Load Collides
            var collideTiles = new List<CollisionTile>();
            foreach (var layer in tiledMap.layers)
            {
                int x_pos = 0;
                int y_pos = 0;
          
                foreach (var tile in layer.data)
                {   
                    var tile_id = tile;
                    tile_id &= ~(FLIPPED_HORIZONTALLY_FLAG |
                                 FLIPPED_VERTICALLY_FLAG   |
                                 FLIPPED_DIAGONALLY_FLAG   );
                    
                    if ((tile_id == 0 && layer.id == tiledMap.layers[0].id) || (tile_id > 0 && IsTileCollide(tile_id, tiledMap)))
                    {
                        var posVec = new Vector2(x_pos*tiledMap.tilewidth, y_pos*tiledMap.tileheight);
                        
                        var tileRec = tile_id == 0 ? new Rectangle() { width = tiledMap.tilewidth, height = tiledMap.tileheight} : GetTileRecById(tile_id, tiledMap);
                                    
                        var resizedTileRec = new Rectangle(posVec.X*IMAGE_SCALE, posVec.Y*IMAGE_SCALE, Math.Abs(tileRec.width)*IMAGE_SCALE, Math.Abs(tileRec.height)*IMAGE_SCALE);
                                               
                        collideTiles.Add(new CollisionTile() { Rec = resizedTileRec, Used = false, TileId = (int)tile_id });
                    }
                    
                    x_pos++;
                    if (x_pos >= layer.width)
                    {
                        x_pos = 0;
                        y_pos++;
                    }
                }
            }

            //Configure collisions more precise
            foreach (var tile1 in collideTiles)
            {
                if (tile1.Used)
                    continue;

                tile1.Used = true;
                    
                var newCollisionRec = tile1.Rec;
                
                if (tile1.TileId > 0)
                {
                    foreach (var tile2 in collideTiles)
                    {
                        if (tile2.Used)
                            continue;

                        if (NihilNetworkUtils.Get2dDistanceBetween(new Vector2(tile2.Rec.x + tile2.Rec.width, tile2.Rec.y),
                                        new Vector2(newCollisionRec.x, newCollisionRec.y)) == 0)
                        {
                            newCollisionRec.x = tile2.Rec.x;
                            
                            newCollisionRec.width += tile2.Rec.width;

                            tile2.Used = true;
                        }
                        else if (NihilNetworkUtils.Get2dDistanceBetween(new Vector2(tile2.Rec.x, tile2.Rec.y + tile2.Rec.height), 
                                            new Vector2(newCollisionRec.x, newCollisionRec.y)) == 0)
                        {
                            newCollisionRec.y = tile2.Rec.y;
                            
                            newCollisionRec.height += tile2.Rec.height;

                            tile2.Used = true;
                        }
                        else if (NihilNetworkUtils.Get2dDistanceBetween(new Vector2(newCollisionRec.x + newCollisionRec.width, newCollisionRec.y), 
                                            new Vector2(tile2.Rec.x, tile2.Rec.y)) == 0)
                        {
                            newCollisionRec.width += tile2.Rec.width;

                            tile2.Used = true;
                        }
                        else if (NihilNetworkUtils.Get2dDistanceBetween(new Vector2(newCollisionRec.x, newCollisionRec.y + newCollisionRec.height), 
                                            new Vector2(tile2.Rec.x, tile2.Rec.y)) == 0)
                        {
                            newCollisionRec.height += tile2.Rec.height;

                            tile2.Used = true;
                        }
                    }
                }
                
                tiledMap.CollideTiles.Add(newCollisionRec);
            }

            _tiledMaps.Add(tiledMap);
        }

        public void EndGame()
        {
            UnloadTexture(_joinButton);
            
            foreach (var tiledMap in _tiledMaps)
                foreach (var tiledMapTexture in tiledMap.TiledMapTextures)
                    UnloadTexture(tiledMapTexture.Texture);
                 
            UnloadSound(_fxButton);

            if (_isServer)
                _gameServer.StopServer();

            _gameClient.Disconnect();
            CloseAudioDevice();
            CloseWindow();
        }
        public void RegisterNetworkFunctions(NihilNetworkClient gameclient)
        {
        } 
    }
}