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
// - the small test scene,
// - the first portal surface!!
public class Game : GameWindow
{
    // Very verbose variables, but I'm new to this, so I'll be verbose so I don't lose myself

    // Main reusable object mesh.
    private int _vertexArrayObject;
    private int _vertexBufferObject;
    private int _elementBufferObject;

    // Main scene shader.
    private int _shaderProgram;

    private int _modelLocation;
    private int _viewLocation;
    private int _projectionLocation;
    private int _lightDirectionLocation;
    private int _objectScaleLocation;
    private int _objectTextureLocation;

    // Textures for the current room objects.
    private int _paddingTexture;
    private int _friendlinessCubeTexture;
    private int _hiTechFloorTexture;

    // Separate mesh used only for the portal surface.
    private int _portalVertexArrayObject;
    private int _portalVertexBufferObject;
    private int _portalElementBufferObject;

    // Separate shader used to draw the portal texture onto the portal quad.
    private int _portalShaderProgram;

    private int _portalModelLocation;
    private int _portalViewLocation;
    private int _portalProjectionLocation;
    private int _portalTextureLocation;

    // Off-screen framebuffer resources.
    private int _portalFramebuffer;
    private int _portalColorTexture;
    private int _portalDepthRenderbuffer;

    // Fixed portal render resolution for now, lower render scale at distance for better performance? Ooo!
    private const int PortalTextureWidth = 1024;
    private const int PortalTextureHeight = 1024;

    private Camera _camera = null!;

    // Temporary second camera just to prove render-to-texture works.
    private Camera _portalCamera = null!;

    private const float CameraMovementSpeed = 3.0f;
    private const float MouseSensitivity = 0.1f;

    // Tick! Of course, it's just tick + deltaTime... Good for floating cubes.
    private float _tick;

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
        CreatePortalSurface();

        _shaderProgram =
            CreateShaderProgram(
                "basic.vert",
                "basic.frag");

        _portalShaderProgram =
            CreateShaderProgram(
                "portal.vert",
                "portal.frag");

        _modelLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "model");

        _viewLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "view");

        _projectionLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "projection");

        _lightDirectionLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "lightDirection");

        // Lets basic.vert calculate how many texture tiles fit across each face.
        _objectScaleLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "objectScale");

        // Texture sampler used by normal room objects.
        _objectTextureLocation =
            GL.GetUniformLocation(
                _shaderProgram,
                "objectTexture");

        _portalModelLocation =
            GL.GetUniformLocation(
                _portalShaderProgram,
                "model");

        _portalViewLocation =
            GL.GetUniformLocation(
                _portalShaderProgram,
                "view");

        _portalProjectionLocation =
            GL.GetUniformLocation(
                _portalShaderProgram,
                "projection");

        _portalTextureLocation =
            GL.GetUniformLocation(
                _portalShaderProgram,
                "portalTexture");

        // Normal object textures use texture unit 0 too.
        GL.UseProgram(_shaderProgram);

        GL.Uniform1(
            _objectTextureLocation,
            0);

        // Tell the portal shader that portalTexture uses texture unit 0.
        GL.UseProgram(_portalShaderProgram);

        GL.Uniform1(
            _portalTextureLocation,
            0);

        GL.UseProgram(0);

        // One complete source image = one complete 1x1 block face.
        _paddingTexture =
            TextureLoader.LoadTexture(
                "padding.png");

        _friendlinessCubeTexture =
            TextureLoader.LoadTexture(
                "friendlinessCube.png");

        _hiTechFloorTexture =
            TextureLoader.LoadTexture(
                "hiTechFloor.png");

        CreatePortalFramebuffer();

        // Player camera.
        _camera = new Camera(
            new Vector3(0.0f, 0.5f, 4.5f),
            ClientSize.X / (float)ClientSize.Y);

        // Fixed camera used for the portal texture.
        // New cameras face -Z, so i is rotated 180 degrees to look back into the room.
        _portalCamera = new Camera(
            new Vector3(0.0f, 0.5f, -4.5f),
            PortalTextureWidth / (float)PortalTextureHeight);

        _portalCamera.Rotate(
            180.0f,
            0.0f);

        CursorState =
            OpenTK.Windowing.Common.CursorState.Grabbed;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteBuffer(_elementBufferObject);
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);

        GL.DeleteBuffer(_portalElementBufferObject);
        GL.DeleteBuffer(_portalVertexBufferObject);
        GL.DeleteVertexArray(_portalVertexArrayObject);

        GL.DeleteProgram(_shaderProgram);
        GL.DeleteProgram(_portalShaderProgram);

        // Delete the normal material textures.
        GL.DeleteTexture(_paddingTexture);
        GL.DeleteTexture(_friendlinessCubeTexture);
        GL.DeleteTexture(_hiTechFloorTexture);

        GL.DeleteRenderbuffer(_portalDepthRenderbuffer);
        GL.DeleteTexture(_portalColorTexture);
        GL.DeleteFramebuffer(_portalFramebuffer);
    }

    // Called repeatedly to update application/game state. Before rendering so logic -> render order is kept.
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

        _tick +=
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

        // First pass, render the room from the fixed portal camera into an off screen framebuffer.
        RenderPortalTexture();

        // Second pass, switch back to the normal window framebuffer.
        GL.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            0);

        GL.Viewport(
            0,
            0,
            ClientSize.X,
            ClientSize.Y);

        GL.Clear(
            ClearBufferMask.ColorBufferBit |
            ClearBufferMask.DepthBufferBit);

        Matrix4 playerView =
            _camera.GetViewMatrix();

        Matrix4 playerProjection =
            _camera.GetProjectionMatrix();

        RenderScene(
            playerView,
            playerProjection);

        // Draw the textured portal surface after the room.
        DrawPortalSurface(
            playerView,
            playerProjection);

        // Aaand to the screen
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

    // Render the room into the framebuffer texture.
    private void RenderPortalTexture()
    {
        GL.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            _portalFramebuffer);

        // Viewport has to match the texture we're rendering into.
        GL.Viewport(
            0,
            0,
            PortalTextureWidth,
            PortalTextureHeight);

        GL.Clear(
            ClearBufferMask.ColorBufferBit |
            ClearBufferMask.DepthBufferBit);

        Matrix4 portalView =
            _portalCamera.GetViewMatrix();

        Matrix4 portalProjection =
            _portalCamera.GetProjectionMatrix();

        // RenderScene does not draw the portal itself... Recursion comes later.
        RenderScene(
            portalView,
            portalProjection);
    }

    // Draw the room with whatever view/projection matrices are supplied.
    private void RenderScene(
        Matrix4 view,
        Matrix4 projection)
    {
        GL.UseProgram(_shaderProgram);

        Vector3 lightDirection =
            Vector3.Normalize(
                new Vector3(-0.4f, 1.0f, 0.3f));

        GL.Uniform3(
            _lightDirectionLocation,
            lightDirection.X,
            lightDirection.Y,
            lightDirection.Z);

        GL.UniformMatrix4(
            _viewLocation,
            true,
            ref view);

        GL.UniformMatrix4(
            _projectionLocation,
            true,
            ref projection);

        GL.BindVertexArray(
            _vertexArrayObject);

        // Good debug to keep, just in case.
        //GL.PointSize(16.0f);

        // Flöör
        DrawObject(
            new Vector3(0.0f, -1.5f, 0.0f),
            new Vector3(12.0f, 0.25f, 12.0f),
            _hiTechFloorTexture);

        // Back wall
        DrawObject(
            new Vector3(0.0f, 1.5f, -6.0f),
            new Vector3(12.0f, 6.0f, 0.25f),
            _paddingTexture);

        // Left wall
        DrawObject(
            new Vector3(-6.0f, 1.5f, 0.0f),
            new Vector3(0.25f, 6.0f, 12.0f),
            _paddingTexture);

        // Right wall
        DrawObject(
            new Vector3(6.0f, 1.5f, 0.0f),
            new Vector3(0.25f, 6.0f, 12.0f),
            _paddingTexture);

        // Crazy cube ahead.
        float floatingCubeHeight =
            0.5f +
            MathF.Cos(_tick) *
            1.25f;

        // Y rotates at full tick speed.
        // X rotates at half the Y axis speed.
        Vector3 floatingCubeRotation =
            new Vector3(
                _tick * 0.5f,
                _tick,
                0.0f);

        DrawObject(
            new Vector3(
                0.0f,
                floatingCubeHeight,
                0.0f),
            Vector3.One,
            floatingCubeRotation,
            _friendlinessCubeTexture);
    }

    // I'm beginning to tire of the word matrix-

    // Convenience version for objects that do NOT rotate. Eg walls.
    private void DrawObject(
        Vector3 position,
        Vector3 scale,
        int texture)
    {
        DrawObject(
            position,
            scale,
            Vector3.Zero,
            texture);
    }

    // Reuse the same VAO/VBO/EBO and only change the model matrix.
    private void DrawObject(
        Vector3 position,
        Vector3 scale,
        Vector3 rotation,
        int texture)
    {
        // Matrix matrix matrix... At least this handles rotation for me.
        Matrix4 model =
            Matrix4.CreateScale(scale) *
            Matrix4.CreateRotationX(rotation.X) *
            Matrix4.CreateRotationY(rotation.Y) *
            Matrix4.CreateRotationZ(rotation.Z) *
            Matrix4.CreateTranslation(position);

        GL.UniformMatrix4(
            _modelLocation,
            true,
            ref model);

        // Send the original object dimensions so the shader can repeat by world size.
        GL.Uniform3(
            _objectScaleLocation,
            MathF.Abs(scale.X),
            MathF.Abs(scale.Y),
            MathF.Abs(scale.Z));

        GL.ActiveTexture(
            TextureUnit.Texture0);

        GL.BindTexture(
            TextureTarget.Texture2D,
            texture);

        GL.DrawElements(
            PrimitiveType.Triangles,
            36,
            DrawElementsType.UnsignedInt,
            0);
    }

    // Draw the textured rectangle that is currently our fake portal.
    private void DrawPortalSurface(
        Matrix4 view,
        Matrix4 projection)
    {
        GL.UseProgram(_portalShaderProgram);

        // Put it slightly in front of the back wall to avoid z fighting.
        Matrix4 portalModel =
            Matrix4.CreateScale(
                2.0f,
                3.0f,
                1.0f)
            *
            Matrix4.CreateTranslation(
                0.0f,
                0.0f,
                -5.85f);

        GL.UniformMatrix4(
            _portalModelLocation,
            true,
            ref portalModel);

        GL.UniformMatrix4(
            _portalViewLocation,
            true,
            ref view);

        GL.UniformMatrix4(
            _portalProjectionLocation,
            true,
            ref projection);

        GL.ActiveTexture(
            TextureUnit.Texture0);

        GL.BindTexture(
            TextureTarget.Texture2D,
            _portalColorTexture);

        GL.BindVertexArray(
            _portalVertexArrayObject);

        GL.DrawElements(
            PrimitiveType.Triangles,
            6,
            DrawElementsType.UnsignedInt,
            0);
    }

    // Create the reusable mesh stored on the GPU. Aka, the cube.
    private void CreateObject()
    {
        // Used to have 8 cube corners, fine for X Y Z R G B, not normals however, so here is this mess
        float[] vertices =
        {
            // X Y Z, R G B, Normals! (X Y Z)
            // U V added at the end for tiling textures.

            // Front (+Z)
            -0.5f, -0.5f,  0.5f,        0.2f, 0.5f, 1.0f,       0.0f,  0.0f,  1.0f,     0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,        0.2f, 0.5f, 1.0f,       0.0f,  0.0f,  1.0f,     1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,        0.2f, 0.5f, 1.0f,       0.0f,  0.0f,  1.0f,     1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,        0.2f, 0.5f, 1.0f,       0.0f,  0.0f,  1.0f,     0.0f, 0.0f,

            // Back (-Z)
             0.5f, -0.5f, -0.5f,        0.8f, 0.2f, 0.2f,       0.0f,  0.0f, -1.0f,     0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,        0.8f, 0.2f, 0.2f,       0.0f,  0.0f, -1.0f,     1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,        0.8f, 0.2f, 0.2f,       0.0f,  0.0f, -1.0f,     1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,        0.8f, 0.2f, 0.2f,       0.0f,  0.0f, -1.0f,     0.0f, 0.0f,

            // Left (-X)
            -0.5f, -0.5f, -0.5f,        0.2f, 0.8f, 0.3f,      -1.0f,  0.0f,  0.0f,     0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,        0.2f, 0.8f, 0.3f,      -1.0f,  0.0f,  0.0f,     1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,        0.2f, 0.8f, 0.3f,      -1.0f,  0.0f,  0.0f,     1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,        0.2f, 0.8f, 0.3f,      -1.0f,  0.0f,  0.0f,     0.0f, 0.0f,

            // Right (+X)
             0.5f, -0.5f,  0.5f,        1.0f, 0.5f, 0.2f,       1.0f,  0.0f,  0.0f,     0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,        1.0f, 0.5f, 0.2f,       1.0f,  0.0f,  0.0f,     1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,        1.0f, 0.5f, 0.2f,       1.0f,  0.0f,  0.0f,     1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,        1.0f, 0.5f, 0.2f,       1.0f,  0.0f,  0.0f,     0.0f, 0.0f,

            // Bottom (-Y)
            -0.5f, -0.5f, -0.5f,        0.6f, 0.3f, 0.8f,       0.0f, -1.0f,  0.0f,     0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,        0.6f, 0.3f, 0.8f,       0.0f, -1.0f,  0.0f,     1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,        0.6f, 0.3f, 0.8f,       0.0f, -1.0f,  0.0f,     1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,        0.6f, 0.3f, 0.8f,       0.0f, -1.0f,  0.0f,     0.0f, 0.0f,

            // Top (+Y)
            -0.5f,  0.5f,  0.5f,        0.8f, 0.8f, 0.8f,       0.0f,  1.0f,  0.0f,     0.0f, 1.0f,
             0.5f,  0.5f,  0.5f,        0.8f, 0.8f, 0.8f,       0.0f,  1.0f,  0.0f,     1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,        0.8f, 0.8f, 0.8f,       0.0f,  1.0f,  0.0f,     1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,        0.8f, 0.8f, 0.8f,       0.0f,  1.0f,  0.0f,     0.0f, 0.0f
        };

        uint[] indices =
        {
             0,  1,  2,   2,  3,  0, // Front
             4,  5,  6,   6,  7,  4, // Back
             8,  9, 10,  10, 11,  8, // Left
            12, 13, 14,  14, 15, 12, // Right
            16, 17, 18,  18, 19, 16, // Bottom
            20, 21, 22,  22, 23, 20  // Top
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
            BufferUsageHint.StaticDraw); // This means WE DRAW THIS SAME OBJECT (to clarify) MANY TIMES, REMEMBER

        _elementBufferObject = GL.GenBuffer();

        GL.BindBuffer(
            BufferTarget.ElementArrayBuffer,
            _elementBufferObject);

        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            indices.Length * sizeof(uint),
            indices,
            BufferUsageHint.StaticDraw);

        // 3 position + 3 color + 3 normal + 2 UV.
        int vertexStride =
            11 * sizeof(float);

        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            vertexStride,
            0);

        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(
            1,
            3,
            VertexAttribPointerType.Float,
            false,
            vertexStride,
            3 * sizeof(float));

        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(
            2,
            3,
            VertexAttribPointerType.Float,
            false,
            vertexStride,
            6 * sizeof(float));

        GL.EnableVertexAttribArray(2);

        // Attribute 3 = UV.
        GL.VertexAttribPointer(
            3,
            2,
            VertexAttribPointerType.Float,
            false,
            vertexStride,
            9 * sizeof(float));

        GL.EnableVertexAttribArray(3);

        GL.BindVertexArray(0);
    }

    // Dedicated portal rectangle with UV coordinates.
    private void CreatePortalSurface()
    {
        float[] vertices =
        {
            // X      Y      Z       U     V
            -0.5f,  -0.5f,  0.0f,   0.0f, 0.0f,
             0.5f,  -0.5f,  0.0f,   1.0f, 0.0f,
             0.5f,   0.5f,  0.0f,   1.0f, 1.0f,
            -0.5f,   0.5f,  0.0f,   0.0f, 1.0f
        };

        uint[] indices =
        {
            0, 1, 2,
            2, 3, 0
        };

        _portalVertexArrayObject =
            GL.GenVertexArray();

        GL.BindVertexArray(
            _portalVertexArrayObject);

        _portalVertexBufferObject =
            GL.GenBuffer();

        GL.BindBuffer(
            BufferTarget.ArrayBuffer,
            _portalVertexBufferObject);

        GL.BufferData(
            BufferTarget.ArrayBuffer,
            vertices.Length * sizeof(float),
            vertices,
            BufferUsageHint.StaticDraw);

        _portalElementBufferObject =
            GL.GenBuffer();

        GL.BindBuffer(
            BufferTarget.ElementArrayBuffer,
            _portalElementBufferObject);

        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            indices.Length * sizeof(uint),
            indices,
            BufferUsageHint.StaticDraw);

        int portalVertexStride =
            5 * sizeof(float);

        // Attribute 0 = position.
        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            portalVertexStride,
            0);

        GL.EnableVertexAttribArray(0);

        // Attribute 1 = UV.
        GL.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            portalVertexStride,
            3 * sizeof(float));

        GL.EnableVertexAttribArray(1);

        GL.BindVertexArray(0);
    }

    // Create the off-screen framebuffer whose color output is a texture.
    private void CreatePortalFramebuffer()
    {
        _portalFramebuffer =
            GL.GenFramebuffer();

        GL.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            _portalFramebuffer);

        // COLOR ATTACHMENT.
        _portalColorTexture =
            GL.GenTexture();

        GL.BindTexture(
            TextureTarget.Texture2D,
            _portalColorTexture);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgb,
            PortalTextureWidth,
            PortalTextureHeight,
            0,
            PixelFormat.Rgb,
            PixelType.UnsignedByte,
            IntPtr.Zero);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        GL.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            _portalColorTexture,
            0);

        _portalDepthRenderbuffer =
            GL.GenRenderbuffer();

        GL.BindRenderbuffer(
            RenderbufferTarget.Renderbuffer,
            _portalDepthRenderbuffer);

        GL.RenderbufferStorage(
            RenderbufferTarget.Renderbuffer,
            RenderbufferStorage.DepthComponent24,
            PortalTextureWidth,
            PortalTextureHeight);

        GL.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            _portalDepthRenderbuffer);

        FramebufferErrorCode framebufferStatus =
            GL.CheckFramebufferStatus(
                FramebufferTarget.Framebuffer);

        if (framebufferStatus !=
            FramebufferErrorCode.FramebufferComplete)
        {
            throw new Exception(
                $"Portal framebuffer is incomplete: {framebufferStatus}");
        }

        GL.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            0);
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
            string error =
                GL.GetShaderInfoLog(shader);

            GL.DeleteShader(shader);

            throw new Exception(
                $"{type} compilation failed:\n{error}");
        }

        return shader;
    }

    // Generic shader-program creator.
    // I now have:
    // - basic.vert/basic.frag for lit 3D objects
    // - portal.vert/portal.frag for the framebuffer texture
    private static int CreateShaderProgram(
        string vertexShaderFile,
        string fragmentShaderFile)
    {
        string vertexSource =
            File.ReadAllText(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Shaders",
                    vertexShaderFile));

        string fragmentSource =
            File.ReadAllText(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Shaders",
                    fragmentShaderFile));

        int vertexShader =
            CompileShader(
                ShaderType.VertexShader,
                vertexSource);

        int fragmentShader =
            CompileShader(
                ShaderType.FragmentShader,
                fragmentSource);

        int shaderProgram =
            GL.CreateProgram();

        GL.AttachShader(
            shaderProgram,
            vertexShader);

        GL.AttachShader(
            shaderProgram,
            fragmentShader);

        GL.LinkProgram(
            shaderProgram);

        GL.GetProgram(
            shaderProgram,
            GetProgramParameterName.LinkStatus,
            out int success);

        if (success == 0)
        {
            string error =
                GL.GetProgramInfoLog(
                    shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteProgram(shaderProgram);

            throw new Exception(
                $"Shader linking failed:\n{error}");
        }

        GL.DetachShader(
            shaderProgram,
            vertexShader);

        GL.DetachShader(
            shaderProgram,
            fragmentShader);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return shaderProgram;
    }
}
