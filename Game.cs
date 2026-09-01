using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.IO;

namespace PortalPrototype;

public class Game : GameWindow
{
    // Very verbose variables, but I'm new to this, so I'll be verbose so I don't lose myself
    private int _vertexArrayObject;
    private int _vertexBufferObject;
    private int _elementBufferObject;
    private int _shaderProgram;

    private Camera _camera = null!;

    private const float CameraMovementSpeed = 3.0f;
    private const float MouseSensitivity = 0.1f;

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

        GL.Enable(EnableCap.DepthTest);

        GL.ClearColor(
            0.1f,
            0.1f,
            0.2f,
            1.0f);

        CreateObject();
        CreateShaderProgram();

        _camera = new Camera(
            new Vector3(0.0f, 0.0f, 3.0f),
            ClientSize.X / (float)ClientSize.Y);

        CursorState =
            OpenTK.Windowing.Common.CursorState.Grabbed;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteBuffer(_elementBufferObject);
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
        GL.DeleteProgram(_shaderProgram);
    }

    protected override void OnUpdateFrame(
        FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (!IsFocused)
        {
            return;
        }

        float deltaTime =
            (float)args.Time;

        float movementAmount =
            CameraMovementSpeed *
            deltaTime;

        if (KeyboardState.IsKeyDown(
            Keys.Escape))
        {
            Close();
        }

        if (KeyboardState.IsKeyDown(
            Keys.W))
        {
            _camera.Position +=
                _camera.Front *
                movementAmount;
        }

        if (KeyboardState.IsKeyDown(
            Keys.S))
        {
            _camera.Position -=
                _camera.Front *
                movementAmount;
        }

        if (KeyboardState.IsKeyDown(
            Keys.A))
        {
            _camera.Position -=
                _camera.Right *
                movementAmount;
        }

        if (KeyboardState.IsKeyDown(
            Keys.D))
        {
            _camera.Position +=
                _camera.Right *
                movementAmount;
        }

        if (KeyboardState.IsKeyDown(
            Keys.Space))
        {
            _camera.Position +=
                Vector3.UnitY *
                movementAmount;
        }

        if (
            KeyboardState.IsKeyDown(
                Keys.LeftControl)
            ||
            KeyboardState.IsKeyDown(
                Keys.C))
        {
            _camera.Position -=
                Vector3.UnitY *
                movementAmount;
        }

        Vector2 mouseDelta =
            MouseState.Delta;

        _camera.Rotate(
            mouseDelta.X *
                MouseSensitivity,

            -mouseDelta.Y *
                MouseSensitivity);
    }

    protected override void OnRenderFrame(
        FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(
            ClearBufferMask.ColorBufferBit |
            ClearBufferMask.DepthBufferBit);

        GL.UseProgram(_shaderProgram);

        Matrix4 model =
            Matrix4.CreateRotationX(
                MathHelper.DegreesToRadians(-20.0f))
            *
            Matrix4.CreateRotationY(
                MathHelper.DegreesToRadians(35.0f));

        Matrix4 view =
            _camera.GetViewMatrix();

        Matrix4 projection =
            _camera.GetProjectionMatrix();

        int modelLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "model");

        int viewLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "view");

        int projectionLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "projection");

        GL.UniformMatrix4(
            modelLocation,
            true,
            ref model);

        GL.UniformMatrix4(
            viewLocation,
            true,
            ref view);

        GL.UniformMatrix4(
            projectionLocation,
            true,
            ref projection);

        GL.BindVertexArray(
            _vertexArrayObject);

        //GL.PointSize(16.0f);

        GL.DrawElements(
            PrimitiveType.Triangles,
            36,
            DrawElementsType.UnsignedInt,
            0);

        SwapBuffers();
    }

    protected override void OnResize(
        ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(
            0,
            0,
            e.Width,
            e.Height);

        if (_camera is not null &&
            e.Height > 0)
        {
            _camera.AspectRatio =
                e.Width / (float)e.Height;
        }
    }

    private void CreateObject()
    {
        float[] vertices =
        {
            // X Y Z R G B

            // Back
            -0.5f, -0.5f, -0.5f,    0.0f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,     1.0f, 0.0f, 0.0f,
            0.5f,  0.5f, -0.5f,     1.0f, 1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,    0.0f, 1.0f, 0.0f,

            // Front
            -0.5f, -0.5f,  0.5f,    0.0f, 0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,     1.0f, 0.0f, 1.0f,
            0.5f,  0.5f,  0.5f,     1.0f, 1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,    0.0f, 1.0f, 1.0f
        };

        uint[] indices =
        {
            // Back
            0, 1, 2,
            2, 3, 0,

            // Front
            4, 5, 6,
            6, 7, 4,

            // Left
            0, 4, 7,
            7, 3, 0,

            // Right
            1, 5, 6,
            6, 2, 1,

            // Bottom
            0, 1, 5,
            5, 4, 0,

            // Top
            3, 2, 6,
            6, 7, 3
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

        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(
            BufferTarget.ElementArrayBuffer,
            _elementBufferObject);

        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            indices.Length * sizeof(uint),
            indices,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            6 * sizeof(float),
            0);

        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(
            1,
            3,
            VertexAttribPointerType.Float,
            false,
            6 * sizeof(float),
            3 * sizeof(float));

        GL.EnableVertexAttribArray(1);

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

        GL.AttachShader(
            _shaderProgram,
            vertexShader);

        GL.AttachShader(
            _shaderProgram,
            fragmentShader);

        GL.LinkProgram(_shaderProgram);

        GL.GetProgram(
            _shaderProgram,
            GetProgramParameterName.LinkStatus,
            out int success);

        if (success == 0)
        {
            string error =
                GL.GetProgramInfoLog(
                    _shaderProgram);

            throw new Exception(
                $"Shader linking failed:\n{error}");
        }

        GL.DetachShader(
            _shaderProgram,
            vertexShader);

        GL.DetachShader(
            _shaderProgram,
            fragmentShader);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }
}