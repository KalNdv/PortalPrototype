using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PortalPrototype;

// This class owns:
// - the OpenGL buffers,
// - the shader program,
// - the camera,
// - input handling,
// - the small test scene.
public class Game : GameWindow
{
    // Very verbose variables, but I'm new to this, so I'll be verbose so I don't lose myself
    private int _vertexArrayObject;
    private int _vertexBufferObject;
    private int _elementBufferObject;
    private int _shaderProgram;

    private int _modelLocation;
    private int _viewLocation;
    private int _projectionLocation;

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

        // Print the OpenGL version supplied by the graphics driver.
        Console.WriteLine(
            $"OpenGL: {GL.GetString(StringName.Version)}");

        // Print the GPU OpenGL is currently rendering with.
        Console.WriteLine(
            $"GPU: {GL.GetString(StringName.Renderer)}");

        GL.Enable(EnableCap.DepthTest);

        GL.ClearColor(
            0.1f,
            0.1f,
            0.2f,
            1.0f);

        CreateObject();

        // Load, compile and link the GLSL shaders.
        CreateShaderProgram();

        // Ask OpenGL where the "model" uniform lives inside the linked shader.
        _modelLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "model");

        // Ask OpenGL where the "view" uniform lives.
        _viewLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "view");

        // Ask OpenGL where the "projection" uniform lives.
        _projectionLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "projection");

        // Create the camera.
        _camera = new Camera(
            new Vector3(0.0f, 0.5f, 4.5f),
            ClientSize.X / (float)ClientSize.Y);

        // Lock/grab the mouse cursor inside the window.
        CursorState =
            OpenTK.Windowing.Common.CursorState.Grabbed;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        // Delete the GPU index buffer.
        GL.DeleteBuffer(_elementBufferObject);

        // Delete the GPU vertex buffer.
        GL.DeleteBuffer(_vertexBufferObject);

        // Delete the vertex layout/configuration object.
        GL.DeleteVertexArray(_vertexArrayObject);

        // Delete the linked shader program.
        GL.DeleteProgram(_shaderProgram);
    }

    // Called repeatedly to update application/game state. Before rendering so logic -> render order is kept.
    protected override void OnUpdateFrame(
        FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // If the window is not focused, do not process movement/mouse input.
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

        // Clear the color buffer so the old image disappears.
        GL.Clear(
            ClearBufferMask.ColorBufferBit |
            ClearBufferMask.DepthBufferBit);

        // Make the shader program active.
        GL.UseProgram(_shaderProgram);

        // world space -> view space.
        Matrix4 view =
            _camera.GetViewMatrix();

        // View space -> clip space with perspective.
        Matrix4 projection =
            _camera.GetProjectionMatrix();

        // Send the view matrix into the shader uniform named "view".
        GL.UniformMatrix4(
            _viewLocation,
            true,
            ref view);

        // Send the projection matrix into the shader uniform named "projection".
        GL.UniformMatrix4(
            _projectionLocation,
            true,
            ref projection);

        // Bind our VAO. OpenGL now knows which VAO to use (mine, of course)
        GL.BindVertexArray(
            _vertexArrayObject);

        // Good debug to keep, just in case.
        //GL.PointSize(16.0f);

        // Flöör
        DrawObject(
            new Vector3(0.0f, -1.5f, 0.0f),
            new Vector3(12.0f, 0.25f, 12.0f));

        // Back wall
        DrawObject(
            new Vector3(0.0f, 1.5f, -6.0f),
            new Vector3(12.0f, 6.0f, 0.25f));

        // Left wall
        DrawObject(
            new Vector3(-6.0f, 1.5f, 0.0f),
            new Vector3(0.25f, 6.0f, 12.0f));

        // Right wall
        DrawObject(
            new Vector3(6.0f, 1.5f, 0.0f),
            new Vector3(0.25f, 6.0f, 12.0f));

        // Cube!!!
        DrawObject(
            new Vector3(-2.5f, -0.75f, -2.5f),
            new Vector3(1.5f, 1.5f, 1.5f));

        // Ceiling reaching cube
        DrawObject(
            new Vector3(2.5f, 0.0f, -3.5f),
            new Vector3(1.0f, 3.0f, 1.0f));

        // Elongated cube
        DrawObject(
            new Vector3(2.0f, -1.0f, 1.5f),
            new Vector3(3.0f, 1.0f, 1.5f));

        // Miniscule cube
        DrawObject(
            new Vector3(-3.5f, -1.1f, 2.0f),
            new Vector3(0.8f, 0.8f, 0.8f));

        // Aand to the screen
        SwapBuffers();
    }

    // Called whenever the window changes size.
    protected override void OnResize(
        ResizeEventArgs e)
    {
        base.OnResize(e);

        // Tell OpenGL how large the drawable viewport is.
        GL.Viewport(
            0,
            0,
            e.Width,
            e.Height);

        // The projection matrix must use the new width/height ratio
        // or the 3D scene will stretch when the window changes shape
        if (_camera is not null &&
            e.Height > 0)
        {
            _camera.AspectRatio =
                e.Width / (float)e.Height;
        }
    }

    // I'm beginning to tire of the word matrix-

    // Reuse the same VAO/VBO/EBO and only change the model matrix.
    private void DrawObject(
        Vector3 position,
        Vector3 scale)
    {
        // scale * translation gives us the object transform I want.
        Matrix4 model =
            Matrix4.CreateScale(scale) *
            Matrix4.CreateTranslation(position);

        // Send this objects model matrix to the shader.
        GL.UniformMatrix4(
            _modelLocation,
            true,
            ref model);

        // Draw 36 indices as triangles.
        // 6*2*3 = 36 indexed tris
        GL.DrawElements(
            PrimitiveType.Triangles,
            36,
            DrawElementsType.UnsignedInt,
            0);
    }

    // Create the reusable mesh stored on the GPU. Aka, the cube.
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

        // Generate a new VAO handle.
        _vertexArrayObject = GL.GenVertexArray();

        // Bind it so the following vertex configuration is stored in this VAO.
        GL.BindVertexArray(_vertexArrayObject);

        // Generate the VBO.
        _vertexBufferObject = GL.GenBuffer();

        // Bind it as the current vertex-array-data buffer.
        GL.BindBuffer(
            BufferTarget.ArrayBuffer,
            _vertexBufferObject);

        // Upload the C# vertex array into the currently bound VBO.
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            vertices.Length * sizeof(float),
            vertices,
            BufferUsageHint.StaticDraw); // This means WE DRAW THIS MANY TIMES, REMEMBER

        // Generate the index/element buffer.
        _elementBufferObject = GL.GenBuffer();

        // Bind it while the VAO is active.
        GL.BindBuffer(
            BufferTarget.ElementArrayBuffer,
            _elementBufferObject);

        // Upload the cubes triangle indices into the EBO.
        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            indices.Length * sizeof(uint),
            indices,
            BufferUsageHint.StaticDraw);

        // Configure vertex attribute 0 = POSITION.
        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            6 * sizeof(float),
            0);

        // Enable shader input location 0.
        GL.EnableVertexAttribArray(0);

        // Configure vertex attribute 1 = COLOR.
        GL.VertexAttribPointer(
            1,
            3,
            VertexAttribPointerType.Float,
            false,
            6 * sizeof(float),
            3 * sizeof(float));

        // Enable shader input location 1.
        GL.EnableVertexAttribArray(1);

        // Unbind the VAO after setup.
        GL.BindVertexArray(0);
    }

    // Compiles one GLSL shader.
    private static int CompileShader(
        ShaderType type,
        string source)
    {
        // Ask OpenGL to create an empty shader object of the requested type.
        int shader = GL.CreateShader(type);

        // Give OpenGL the GLSL source code.
        GL.ShaderSource(shader, source);

        // Ask the graphics driver to compile it.
        GL.CompileShader(shader);

        // Query whether compilation succeeded.
        GL.GetShader(
            shader,
            ShaderParameter.CompileStatus,
            out int success);

        // 0 means compilation failed.
        if (success == 0)
        {
            // Get the drivers GLSL compiler error message...
            string error = GL.GetShaderInfoLog(shader);

            // Stop with a readable error, rather than continuing.
            throw new Exception(
                $"{type} compilation failed:\n{error}");
        }

        return shader;
    }

    // Loads basic.vert and basic.frag, compiles them,
    // links them together into one usable shader program,
    // then removes the temporary individual shader objects.
    private void CreateShaderProgram()
    {
        // Read the vertex shader source from the copied build-output folder.
        string vertexSource = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Shaders",
                "basic.vert"));

        // Read the fragment shader source.
        string fragmentSource = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Shaders",
                "basic.frag"));

        // Compile the vertex shader.
        int vertexShader = CompileShader(
            ShaderType.VertexShader,
            vertexSource);

        // Compile the fragment shader.
        int fragmentShader = CompileShader(
            ShaderType.FragmentShader,
            fragmentSource);

        // Create an empty shader program.
        _shaderProgram = GL.CreateProgram();

        // Attach the compiled vertex shader to the program.
        GL.AttachShader(
            _shaderProgram,
            vertexShader);

        // Attach the compiled fragment shader.
        GL.AttachShader(
            _shaderProgram,
            fragmentShader);

        // Link the stages together.
        GL.LinkProgram(_shaderProgram);

        // Check whether linking succeeded.
        GL.GetProgram(
            _shaderProgram,
            GetProgramParameterName.LinkStatus,
            out int success);

        if (success == 0)
        {
            // Retrieve the error message again.
            string error =
                GL.GetProgramInfoLog(
                    _shaderProgram);

            throw new Exception(
                $"Shader linking failed:\n{error}");
        }

        // Once the program is successfully linked, the individual shader objects no longer need to stay attached.
        GL.DetachShader(
            _shaderProgram,
            vertexShader);

        GL.DetachShader(
            _shaderProgram,
            fragmentShader);

        // Delete the temporary compiled shader objects.
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }
}
