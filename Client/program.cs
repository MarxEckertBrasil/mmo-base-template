using System;
using NihilNetwork;
using NihilNetwork.Converter;
using NihilNetwork.Client;
using rpg_base_template.Client.System;

namespace rpg_base_template.Client
{
    public class Program
    {
        static void Main()
        {
            var gameclient = new NihilNetworkClient();
            gameclient.Connect("127.0.0.1", 1118, 1119, 1120, gameclient, false, true);

            var player = new Player() { name = "teste" };
            var playerConnection = new NihilNetworkObject();
            playerConnection.OwnerConnection = gameclient.Connection.Ip;

            NihilNetworkConverters.ClassToNetworkObject(player, playerConnection);
            gameclient.UpdateNihilNetworkConnection(playerConnection);

            while (gameclient.Connection.Connected)
            {
                Console.WriteLine("client is connected");
                gameclient.Listen();           
            }
            
        }
    }
}