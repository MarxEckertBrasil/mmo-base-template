using System;
using NihilNetwork.Client;

namespace rpg_base_template.Client
{
    public class Program
    {
        static void Main()
        {
            var gameclient = new NihilNetworkClient();
            gameclient.Connect("127.0.0.1", 1118, 1119, 1120, gameclient);

            while (gameclient.Connection.Connected)
            {
                Console.WriteLine("client is connected");
                gameclient.Listen();           
            }
            
        }
    }
}