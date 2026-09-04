#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTextureCoordinate;

out vec2 textureCoordinate;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // Keep the same matrix convention as the rest of this OpenTK project.
    gl_Position =
        vec4(aPosition, 1.0)
        *
        model
        *
        view
        *
        projection;

    textureCoordinate =
        aTextureCoordinate;
}
