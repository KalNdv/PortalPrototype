#version 330 core

in vec3 vertexColor;
in vec3 worldNormal;

out vec4 FragColor;

// Direction from the surface toward the directional light source.
uniform vec3 lightDirection;

void main()
{
    // Interpolated normals may no longer have length 1, so this normalizes again before using the dot product.
    vec3 normal =
        normalize(worldNormal);

    vec3 light =
        normalize(lightDirection);

    // dot(N, L) tells us how aligned the surface normal is with the light.
    //  1 = directly facing the light
    //  0 = perpendicular to the light
    // <0 = facing away from the light
    float diffuseStrength =
        max(
            dot(normal, light),
            0.0);

    // A small constant ambient term stops unlit faces from becoming pure black, so basically the most basic "simulated version of ambient light bouncing for toddlers"
    float ambientStrength =
        0.20;

    // Keep the total brightness between roughly 20% and 100%.
    float brightness =
        ambientStrength +
        diffuseStrength * 0.80;

    vec3 finalColor =
        vertexColor *
        brightness;

    FragColor =
        vec4(
            finalColor,
            1.0);
}
