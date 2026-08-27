using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace PortalPrototype;

internal static class Program
{
    private static void Main()
    {
        GameWindowSettings gameSettings = GameWindowSettings.Default;

        NativeWindowSettings windowSettings = new()
        {
            ClientSize = new Vector2i(1280, 720),
            Title = "Portal Prototype",
            API = ContextAPI.OpenGL,
            APIVersion = new Version(3, 3),
            Profile = ContextProfile.Core
        };

        using Game game = new(gameSettings, windowSettings);
        game.Run();
    }
}