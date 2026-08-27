using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace PortalPrototype;

public class Game : GameWindow
{
    // Very verbose variables, but I'm new to this, so I'll be verbose so I don't lose myself
    private int _vertexArrayObject;
    private int _vertexBufferObject;
    private int _shaderProgram;

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

        CreateTriangle();
        CreateShaderProgram();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vertexArrayObject);

        //GL.PointSize(16.0f);

        GL.DrawArrays(
            PrimitiveType.Triangles,
            0,
            3);

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

    private void CreateTriangle()
    {
        float[] vertices =
        {
        // X      Y      Z
        -0.5f, -0.5f, 0.0f,
         0.0f,  0.5f, 0.0f
    };

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(
            BufferTarget.ArrayBuffer,
            _vertexBufferObject);

        GL.BufferData(
            BufferTarget.ArrayBuffer,
            vertices.Length * sizeof(float),
            vertices,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            3 * sizeof(float),
            0);

        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }

    private static int CompileShader(
    ShaderType type,
    string source)
    {
        int shader = GL.CreateShader(type);

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(
            shader,
            ShaderParameter.CompileStatus,
            out int success);

        if (success == 0)
        {
            string error = GL.GetShaderInfoLog(shader);

            throw new Exception(
                $"{type} compilation failed:\n{error}");
        }

        return shader;
    }

    private void CreateShaderProgram()
    {
        string vertexSource = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Shaders",
                "basic.vert"));

        string fragmentSource = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Shaders",
                "basic.frag"));

        int vertexShader = CompileShader(
            ShaderType.VertexShader,
            vertexSource);

        int fragmentShader = CompileShader(
            ShaderType.FragmentShader,
            fragmentSource);

        _shaderProgram = GL.CreateProgram();

        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);

        GL.LinkProgram(_shaderProgram);

        GL.GetProgram(
            _shaderProgram,
            GetProgramParameterName.LinkStatus,
            out int success);

        if (success == 0)
        {
            string error =
                GL.GetProgramInfoLog(_shaderProgram);

            throw new Exception(
                $"Shader linking failed:\n{error}");
        }

        GL.DetachShader(_shaderProgram, vertexShader);
        GL.DetachShader(_shaderProgram, fragmentShader);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }
}


