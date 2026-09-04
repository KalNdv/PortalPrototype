using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace PortalPrototype;

// Just loads an image into OpenGL and gives me the texture handle back.
public static class TextureLoader
{
    public static int LoadTexture(
        string fileName)
    {
        string texturePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Textures",
                fileName);

        if (!File.Exists(texturePath))
        {
            throw new FileNotFoundException(
                $"Texture was not found: {texturePath}");
        }

        // Decode as RGBA, but keep whatever actual resolution I drew it at.
        using FileStream stream =
            File.OpenRead(texturePath);

        ImageResult image =
            ImageResult.FromStream(
                stream,
                ColorComponents.RedGreenBlueAlpha);

        int texture =
            GL.GenTexture();

        GL.BindTexture(
            TextureTarget.Texture2D,
            texture);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            image.Width,
            image.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            image.Data);

        // Pixel art = nearest. No blur please.
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);

        // UV above 1 loops the image instead of stretching one copy forever.
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.Repeat);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.Repeat);

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);

        Console.WriteLine(
            $"Loaded texture: {fileName} ({image.Width}x{image.Height})");

        return texture;
    }
}
