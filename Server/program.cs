using System;
using NihilNetwork.Server;

namespace rpg_base_template.Server
{
    class Program
    {
        
        static void Main()
        {
            var gameServer = new GameServer();

            var networkServer = new NihilNetworkServer();
            networkServer.StartServer("127.0.0.1", 1118, 1119, 1120, gameServer, 40, false, true);
            
            while (networkServer.IsServerRunning())
            {
                Console.WriteLine("server is running");
                networkServer.Listen();           

                Console.Write("Junin: ");
                Console.WriteLine(networkServer.GetObjectById(0));
                Console.Write("Clebin: ");
                Console.WriteLine(networkServer.GetObjectById(1));
            }
        }
    }
}