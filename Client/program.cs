namespace rpg_base_template.Client
{
    public class Program
    {
        static void Main()
        {
            var game = new GameClient();
            game.GameLoop();

        }
    }
}