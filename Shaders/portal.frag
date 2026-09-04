#version 330 core

in vec2 textureCoordinate;

out vec4 FragColor;

uniform sampler2D portalTexture;

void main()
{
    // Read the color that pass 1 rendered into the framebuffer texture.
    FragColor =
        texture(
            portalTexture,
            textureCoordinate);
}
