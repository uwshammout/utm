namespace CronBlocks.UTM.Startup;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App app = new App();
        app.Run();
    }
}
