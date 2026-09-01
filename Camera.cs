using System;
using OpenTK;
using OpenTK.Mathematics;

namespace PortalPrototype;

public class Camera
{
    private readonly Vector3 _worldUp = Vector3.UnitY;

    public Vector3 Position { get; set; }

    public Vector3 Front { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }

    public float Yaw { get; private set; } = -90.0f;
    public float Pitch { get; private set; } = 0.0f;

    public float FieldOfViewDegrees { get; set; } = 60.0f;
    public float AspectRatio { get; set; }

    public Camera(
        Vector3 position,
        float aspectRatio)
    {
        Position = position;
        AspectRatio = aspectRatio;

        UpdateDirectionVectors();
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(
            Position,
            Position + Front,
            Up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(FieldOfViewDegrees),
            AspectRatio,
            0.1f,
            100.0f);
    }

    public void Rotate(
        float yawOffset,
        float pitchOffset)
    {
        Yaw += yawOffset;
        Pitch += pitchOffset;

        Pitch = Math.Clamp(
            Pitch,
            -89.0f,
            89.0f);

        UpdateDirectionVectors();
    }

    private void UpdateDirectionVectors()
    {
        float yawRadians =
            MathHelper.DegreesToRadians(Yaw);

        float pitchRadians =
            MathHelper.DegreesToRadians(Pitch);

        Vector3 front;

        front.X =
            MathF.Cos(yawRadians) *
            MathF.Cos(pitchRadians);

        front.Y =
            MathF.Sin(pitchRadians);

        front.Z =
            MathF.Sin(yawRadians) *
            MathF.Cos(pitchRadians);

        Front = Vector3.Normalize(front);

        Right = Vector3.Normalize(
            Vector3.Cross(
                Front,
                _worldUp));

        Up = Vector3.Normalize(
            Vector3.Cross(
                Right,
                Front));
    }
}