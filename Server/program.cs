using System;
using NihilNetwork.Server;

namespace rpg_base_template.Server
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var gameServer = new GameServer();

            var networkServer = new NihilNetworkServer();
            networkServer.StartServer("127.0.0.1", 1120, gameServer, 120, false, true);
            
            while (true)
            {
                networkServer.Listen();           
            }
        }
    }
}