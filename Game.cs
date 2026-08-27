using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace PortalPrototype;

public class Game : GameWindow
{
    public Game(
        GameWindowSettings gameSettings,
        NativeWindowSettings windowSettings)
        : base(gameSettings, windowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        Console.WriteLine(
            $"OpenGL: {GL.GetString(StringName.Version)}");

        Console.WriteLine(
            $"GPU: {GL.GetString(StringName.Renderer)}");

        GL.ClearColor(
            0.1f,
            0.1f,
            0.2f,
            1.0f);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(
            0,
            0,
            e.Width,
            e.Height);
    }
}