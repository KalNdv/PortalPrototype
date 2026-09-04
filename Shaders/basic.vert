#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in vec2 aTextureCoordinate;

out vec3 vertexColor;
out vec3 worldNormal;
out vec2 textureCoordinate;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

// Original scale before the object gets transformed.
uniform vec3 objectScale;

void main()
{
    // This project uses OpenTK row-vector style
    // together with GL.UniformMatrix4(..., true, ref matrix).

    vec4 worldPosition =
        vec4(aPosition, 1.0) *
        model;

    gl_Position =
        worldPosition *
        view *
        projection;

    // Correct normals when objects are stretched unevenly.
    mat3 normalMatrix =
        transpose(
            inverse(
                mat3(model)));

    worldNormal =
        normalize(
            aNormal *
            normalMatrix);

    vertexColor = aColor;

    // Pick the two dimensions that actually lie across this face.
    vec3 absoluteNormal =
        abs(aNormal);

    vec2 textureRepeat;

    if (absoluteNormal.z > 0.5)
    {
        textureRepeat =
            vec2(
                objectScale.x,
                objectScale.y);
    }
    else if (absoluteNormal.x > 0.5)
    {
        textureRepeat =
            vec2(
                objectScale.z,
                objectScale.y);
    }
    else
    {
        textureRepeat =
            vec2(
                objectScale.x,
                objectScale.z);
    }

    textureCoordinate =
        aTextureCoordinate *
        textureRepeat;
}
