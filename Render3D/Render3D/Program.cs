using OpenTK.Windowing.Desktop;
namespace Render3D;

class Program
{
    static void Main(string[] Args)
    {
        var NativeWindowSettings = new NativeWindowSettings()
        {
            Size = new OpenTK.Mathematics.Vector2i(1600,800),
            //Size = new OpenTK.Mathematics.Vector2i(800,600),
            Title = "Render3D",
            //NumberOfSamples = 8,
            NumberOfSamples = 1
        };

        using (var Window = new Window(GameWindowSettings.Default, NativeWindowSettings))
        {
            Window.Run();
        }
    }
}
