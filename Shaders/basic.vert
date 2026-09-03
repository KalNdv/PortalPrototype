#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor;
layout(location = 2) in vec3 aNormal;

out vec3 vertexColor;
out vec3 worldNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

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
}