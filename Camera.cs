using System;
using OpenTK;
using OpenTK.Mathematics;

namespace PortalPrototype;

public class Camera
{
    private readonly Vector3 _worldUp = Vector3.UnitY;

    public Vector3 Position { get; set; }

    // The direction the camera is looking, normalized to 1, useful for "where we're moving and looking"
    public Vector3 Front { get; private set; }

    // The camera's current upward direction, != _worldUp, as this is dependent on camera orientation. Basically, this = local, world = global up. Also useful for strafing up and down.
    public Vector3 Up { get; private set; }

    // Camera right, same as front but for A and D strafing
    public Vector3 Right { get; private set; }

    // Yaw = rotation around the world's vertical Y axis. Faces -90, common convention. Why? No clue.
    public float Yaw { get; private set; } = -90.0f;

    // Pitch = rotation up/down.
    public float Pitch { get; private set; } = 0.0f;

    // Vertical field of view in degrees.
    public float FieldOfViewDegrees { get; set; } = 90.0f;

    // Width / height of the render area.
    public float AspectRatio { get; set; }

    // Camera constructor.
    public Camera(
        Vector3 position,
        float aspectRatio)
    {
        Position = position;
        AspectRatio = aspectRatio;

        // Front/Right/Up must be valid before the camera is used.
        UpdateDirectionVectors();
    }

    // Creates the view matrix.
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(
            Position,
            Position + Front,
            Up);
    }

    // Creates the projection matrix.
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(FieldOfViewDegrees),
            AspectRatio,
            0.1f,
            100.0f);
    }

    // Rotates the camera by changing yaw and pitch.
    public void Rotate(
        float yawOffset,
        float pitchOffset)
    {
        Yaw += yawOffset;
        Pitch += pitchOffset;

        // Prevent pitch from reaching exactly +/-90 degrees, will mess up rotation if allowed further, and flip weirdly.
        Pitch = Math.Clamp(
            Pitch,
            -89.0f,
            89.0f);

        // Yaw/pitch changed, so Front/Right/Up must be recalculated.
        UpdateDirectionVectors();
    }

    private void UpdateDirectionVectors()
    {
        float yawRadians =
            MathHelper.DegreesToRadians(Yaw);

        float pitchRadians =
            MathHelper.DegreesToRadians(Pitch);

        // Temporary direction vector before normalization.
        Vector3 front;

        // Convert spherical-style yaw/pitch angles into a Cartesian direction.
        front.X =
            MathF.Cos(yawRadians) *
            MathF.Cos(pitchRadians);

        front.Y =
            MathF.Sin(pitchRadians);

        front.Z =
            MathF.Sin(yawRadians) *
            MathF.Cos(pitchRadians);

        // Normalize makes the vector length exactly 1 fopr consistency.
        Front = Vector3.Normalize(front);

        // Cross product gives a vector perpendicular to both inputs.
        Right = Vector3.Normalize(
            Vector3.Cross(
                Front,
                _worldUp));

        // Right x Front gives us the cameras corrected Up direction.
        Up = Vector3.Normalize(
            Vector3.Cross(
                Right,
                Front));
    }
}
