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
            networkServer.StartServer(1120, gameServer, 10, false, true);
            
            while (true)
            {
                networkServer.Listen();           
            }
        }
    }
}