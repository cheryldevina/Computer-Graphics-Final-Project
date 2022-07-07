using OpenTK.Mathematics;

namespace Render3D
{
    class Camera
    {
        public Vector3 Up;
        public Vector3 Front;
        public Vector3 Position;
        public float FOV;
        public float Yaw;
        public float Pitch;
        public float Far;
        public float Near;
        public float AspectRatio;
        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;

        public Camera(float FOV, float AspectRatio, float Near, float Far)
        {
            this.FOV = FOV;
            this.Far = Far;
            this.Near = Near;
            this.AspectRatio = AspectRatio;
            Up = new Vector3(0, 1, 0);
            Front = new Vector3(0, 0, -1);
            Position = Vector3.Zero;
            Yaw = 0;
            Pitch = 0;

            RefreshLookAt();
            RefreshProjectionMatrix();
        }

        public void LookAt(Vector3 To)
        {
            Front = To;
            ViewMatrix = Matrix4.LookAt(Position, To, Up);
        }

        public void RefreshLookAt()
        {
            ViewMatrix = Matrix4.LookAt(Position, Position + Front, Up);
        }

        public void RefreshProjectionMatrix()
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV.Rad(), AspectRatio, Near, Far);
        }
    }
}
