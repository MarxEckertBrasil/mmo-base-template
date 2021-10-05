using System;
using NihilNetwork.Server;

namespace rpg_base_template.Server
{
    class Program
    {
        
        static void Main()
        {
            var gameserver = new NihilNetworkServer();
            gameserver.StartServer("127.0.0.1", 1118, 1119, 1120, gameserver);

            while (gameserver.IsServerRunning())
            {
                Console.WriteLine("server is running");
                gameserver.Listen();           
            }
        }
    }
}