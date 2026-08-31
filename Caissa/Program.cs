namespace Caissa;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Temporary engine test
        Chess_Engine.EngineTest.Run();

        // Start the actual game
        Application.Run(new MainMenu());
    }
}