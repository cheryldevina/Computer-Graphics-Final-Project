using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Render3D
{
    class PlayerController
    {
        public Model Character;
        public Camera Camera;
        public Vector3 Up;
        public Vector3 Front;
        public Vector3 Shift;
        public Vector3 Position;
        public Vector3 CurrentTranslation;
        public float Pitch;
        public float Yaw;
        public Vector3 Offset;
        public float Velocity;
        public bool IsJumping;
        public bool Flying;
        public bool ThirdPerson;
        public Vector3 CurrentOffset;
        public bool Transition;
        public bool Collided;
        public List<float> OriginalTPS;
        public List<float> CurrentTPS;

        public PlayerController(Model CharacterModel, Camera Camera)
        {
            Character = CharacterModel;
            this.Camera = Camera;
            Velocity = 0;
            Pitch = 0;
            Yaw = 0;
            IsJumping = true;
            ThirdPerson = false;
            CurrentOffset = Vector3.Zero;
            Collided = true;
            Transition = false;
            Flying = false;

            Up = new Vector3(0, 1, 0);
            Front = new Vector3(0, 0, -1);
            Shift = Vector3.Zero;
            Offset = new Vector3(0, 0.85f, 0);
            Position = new Vector3(0, 1, 0);
            CurrentTranslation = Vector3.Zero;

            Character.Meshes.ForEach(x => x.Collision = false);
            Character.CalculateMinMax = true;

            OriginalTPS = new List<float>();
            CurrentTPS = new List<float>();

            foreach (var Timeline in Character.Timelines) {
                OriginalTPS.Add(Timeline.Ticker.TPS);
                Timeline.Ticker.TPS = 0;
                CurrentTPS.Add(Timeline.Ticker.TPS);
            }
        }

        public void DoCollider(Mesh CollidingMesh)
        {
            if (CollidingMesh.BoundingBox != null)
            {
                BoundingBox CharacterAABB = Character.BoundingBox.GetTransformedAxisAlignedBoundingBox();
                BoundingBox CollidingMeshAABB = CollidingMesh.BoundingBox.GetTransformedAxisAlignedBoundingBox();

                if (CharacterAABB.IsCollidingAxisAligned(CollidingMeshAABB))
                {
                    Collided = true;
                    // calculate rebound amount
                    Vector3 CollidingDistance = (CharacterAABB.HalfSize + CollidingMeshAABB.HalfSize);
                    Vector3 CurrentDistance = (CharacterAABB.Center - CollidingMeshAABB.Center);
                    Vector3 AbsCurrentDistance = new Vector3(
                        Math.Abs(CurrentDistance.X),
                        Math.Abs(CurrentDistance.Y),
                        Math.Abs(CurrentDistance.Z));
                    Vector3 ShiftAmount = Vector3.Zero;

                    if (AbsCurrentDistance.X < CollidingDistance.X)
                    {
                        float Direction = CurrentDistance.X / AbsCurrentDistance.X;
                        ShiftAmount.X = Direction * (CollidingDistance.X - AbsCurrentDistance.X);
                    }

                    if (AbsCurrentDistance.Y < CollidingDistance.Y)
                    {
                        float Direction = CurrentDistance.Y / AbsCurrentDistance.Y;
                        ShiftAmount.Y = Direction * (CollidingDistance.Y - AbsCurrentDistance.Y);
                    }

                    if (AbsCurrentDistance.Z < CollidingDistance.Z)
                    {
                        float Direction = CurrentDistance.Z / AbsCurrentDistance.Z;
                        ShiftAmount.Z = Direction * (CollidingDistance.Z - AbsCurrentDistance.Z);
                    }

                    // side check

                    float XIntersectRatio = Math.Max(
                        BoundingBox.FaceIntersectionRatioAxisAligned(CharacterAABB, BoundingFace.MinX, CollidingMeshAABB, BoundingFace.MaxX),
                        BoundingBox.FaceIntersectionRatioAxisAligned(CharacterAABB, BoundingFace.MaxX, CollidingMeshAABB, BoundingFace.MinX));

                    float YIntersectRatio = Math.Max(
                        BoundingBox.FaceIntersectionRatioAxisAligned(CharacterAABB, BoundingFace.MinY, CollidingMeshAABB, BoundingFace.MaxY),
                        BoundingBox.FaceIntersectionRatioAxisAligned(CharacterAABB, BoundingFace.MaxY, CollidingMeshAABB, BoundingFace.MinY));

                    float ZIntersectRatio = Math.Max(
                        BoundingBox.FaceIntersectionRatioAxisAligned(CharacterAABB, BoundingFace.MinZ, CollidingMeshAABB, BoundingFace.MaxZ),
                        BoundingBox.FaceIntersectionRatioAxisAligned(CharacterAABB, BoundingFace.MaxZ, CollidingMeshAABB, BoundingFace.MinZ));

                    if (XIntersectRatio > YIntersectRatio && XIntersectRatio > ZIntersectRatio)
                    {
                        if (Math.Abs(ShiftAmount.X) > Math.Abs(Shift.X))
                        {
                            Shift.X = ShiftAmount.X;
                        }
                    }
                    else if (ZIntersectRatio > YIntersectRatio && ZIntersectRatio > XIntersectRatio)
                    {
                        if (Math.Abs(ShiftAmount.Z) > Math.Abs(Shift.Z))
                        {
                            Shift.Z = ShiftAmount.Z;
                        }
                    }
                    else
                    {
                        if (Math.Abs(ShiftAmount.Y) > Math.Abs(Shift.Y))
                        {
                            Shift.Y = ShiftAmount.Y;
                        }
                    }
                    //Console.WriteLine("Collided with " + CollidingMesh.Name);
                    //Console.WriteLine("Collider: " + CollidingMeshAABB.HalfSize.ToString() + " " + CollidingMeshAABB.Center.ToString());
                    //Console.WriteLine("Character: " + CharacterAABB.HalfSize.ToString() + " " + CharacterAABB.Center.ToString());
                    //Console.WriteLine("Combined Half Size: " + CollidingDistance.ToString());
                    //Console.WriteLine("Distance: " + CurrentDistance.ToString());
                    //Console.WriteLine("Shift: " + Shift.ToString());
                    //Console.WriteLine("IOUS: " + XIntersectRatio + " " + YIntersectRatio + " "  + ZIntersectRatio);
                }
            }
        }

        public void WalkAnimation() {
            for (int i = 0; i < OriginalTPS.Count; i++)
            {
                CurrentTPS[i] = OriginalTPS[i];
            }
        }

        public void ResetWalkAnimation() {
            for (int i = 0; i < OriginalTPS.Count; i++)
            {
                CurrentTPS[i] = 0;
            }
        }

        public void Update(FrameEventArgs Args, Render3D.Window Window) 
        {
            ResetWalkAnimation();

            KeyboardState input = Window.KeyboardState;

            if (input.IsKeyDown(Keys.W))
            {
                CurrentTranslation = Front * Constants.SpeedyGonzalez * (float)Args.Time;
                WalkAnimation();
            }

            if (input.IsKeyDown(Keys.S))
            {
                CurrentTranslation = Front * -Constants.SpeedyGonzalez * (float)Args.Time;
                WalkAnimation();
            }

            if (input.IsKeyDown(Keys.A))
            {
                CurrentTranslation = Vector3.Normalize(Vector3.Cross(Front, Up)) * -Constants.SpeedyGonzalez * (float)Args.Time;
                WalkAnimation();
            }

            if (input.IsKeyDown(Keys.D))
            {
                CurrentTranslation = Vector3.Normalize(Vector3.Cross(Front, Up)) * Constants.SpeedyGonzalez * (float)Args.Time;
                WalkAnimation();
            }

            if (input.IsKeyReleased(Keys.F1))
            {
                ThirdPerson = !ThirdPerson;
                Transition = true;
            }

            if (input.IsKeyReleased(Keys.F2))
            {
                Flying = !Flying;
            }

            if (input.IsKeyReleased(Keys.F3))
            {
                Window.DrawBoundingBox = !Window.DrawBoundingBox;
            }

            if (input.IsKeyDown(Keys.Space))
            {
                if (!IsJumping) 
                {
                    Velocity = Constants.JumpHeight;
                    IsJumping = true;
                }
            }

            float DeltaX = Window.MousePosition.X - Window.Size.X / 2f;
            float DeltaY = Window.MousePosition.Y - Window.Size.Y / 2f;

            Yaw += DeltaX * Constants.Sensitivity;
            if (Pitch > 89.0f)
            {
                Pitch = 89.0f;
            }
            else if (Pitch < -89.0f)
            {
                Pitch = -89.0f;
            }
            else
            {
                Pitch -= DeltaY * Constants.Sensitivity;
            }

            Front.X = (float)Math.Cos(Pitch.Rad()) * (float)Math.Cos(Yaw.Rad());
            Front.Y = (float)Math.Sin(Pitch.Rad());
            Front.Z = (float)Math.Cos(Pitch.Rad()) * (float)Math.Sin(Yaw.Rad());
            Front.Normalize();


            if (!Flying)
            {
                CurrentTranslation.Y = 0;
                CurrentTranslation.Y += Velocity * 3 * (float)Args.Time;
                Velocity -= Constants.Gravity * (float)Args.Time;

                if (Collided)
                {
                    if (Math.Abs(Shift.Y) > 0)
                    { 
                        IsJumping = false;
                        CurrentTranslation.Y = 0;
                        Velocity = 0;
                    }
                }

                Position += Shift;
                Collided = false;
            }
            else {
                Velocity = 0;
            }

            Position += CurrentTranslation;
            CurrentTranslation = Vector3.Zero;
            Shift = Vector3.Zero;

            if (ThirdPerson) 
            {
                Vector3 Temp = -Front * Constants.TPDistance + Offset;
                //Temp.Y = Constants.TPDistance;
                //Camera.Position = Position - Temp;

                if ((Temp - CurrentOffset).Length < 0.001f || Transition == false) 
                {
                    Transition = false;
                    CurrentOffset += (Temp - CurrentOffset);
                } else 
                { 
                    CurrentOffset += (Temp - CurrentOffset) * (float)Args.Time * 10f; 
                }
            } else
            {

                if ((Offset - CurrentOffset).Length < 0.001f || Transition == false)
                {
                    Transition = false;
                    CurrentOffset += (Offset - CurrentOffset);
                } else
                {
                    CurrentOffset += (Offset - CurrentOffset) * (float)Args.Time * 10f;
                }
            }

            for(int i = 0; i < OriginalTPS.Count; i++)
            {
                Character.Timelines[i].Ticker.TPS += (CurrentTPS[i] - Character.Timelines[i].Ticker.TPS) * (float)Args.Time * 10f;
            }

            if (Position.Y < -20) 
            {
                Position = new Vector3(0, 10, 0);
                Velocity = 0;
            }

            Vector3 TempFront = new Vector3(Front.X, 0, -Front.Z);
            TempFront = Vector3.Normalize(TempFront);
            Character.ParentNode.PositionVector += (Position - Character.ParentNode.PositionVector) * (float)Args.Time * 10f;
            Character.ParentNode.PositionVector.Y = Position.Y;
            Character.ParentNode.RotationQuatertion = Matrix4.LookAt(Position, Position + TempFront, Up).ExtractRotation();

            Camera.Position = Character.ParentNode.PositionVector + CurrentOffset;
            Camera.Front = Front;
            Camera.RefreshLookAt();
        }
    }
}
