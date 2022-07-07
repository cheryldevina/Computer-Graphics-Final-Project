using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Render3D
{

    class BoundingFace {
        public List<uint> FaceIndices;
        public uint FacePivotIndex;

        public static readonly BoundingFace MinX = new BoundingFace(new List<uint>() { 0, 3, 6, 2 }, 0);
        public static readonly BoundingFace MaxX = new BoundingFace(new List<uint>() { 1, 5, 7, 4 }, 0);

        public static readonly BoundingFace MinY = new BoundingFace(new List<uint>() { 0, 1, 5, 3 }, 1);
        public static readonly BoundingFace MaxY = new BoundingFace(new List<uint>() { 2, 4, 7, 6 }, 1);

        public static readonly BoundingFace MinZ = new BoundingFace(new List<uint>() { 0, 1, 4, 2 }, 2);
        public static readonly BoundingFace MaxZ = new BoundingFace(new List<uint>() { 3, 5, 7, 6 }, 2);

        public BoundingFace(List<uint> FaceIndices, uint FacePivotIndex) 
        {
            this.FaceIndices = FaceIndices;
            this.FacePivotIndex = FacePivotIndex;
        }

        public List<uint> GenerateIndices()
        {
            List<uint> Indices = new List<uint>();

            if (FaceIndices.Count >= 4)
            {
                Indices.Add(FaceIndices[0]);
                Indices.Add(FaceIndices[1]);
                Indices.Add(FaceIndices[2]);

                Indices.Add(FaceIndices[1]);
                Indices.Add(FaceIndices[2]);
                Indices.Add(FaceIndices[3]);

                Indices.Add(FaceIndices[2]);
                Indices.Add(FaceIndices[3]);
                Indices.Add(FaceIndices[0]);

                Indices.Add(FaceIndices[0]);
                Indices.Add(FaceIndices[1]);
                Indices.Add(FaceIndices[3]);
            }

            return Indices;
        }

        public static List<uint> GenerateIndices(BoundingFace Face)
        {
            List<uint> Indices = new List<uint>();

            if (Face.FaceIndices.Count >= 4)
            {
                Indices.Add(Face.FaceIndices[0]);
                Indices.Add(Face.FaceIndices[1]);
                Indices.Add(Face.FaceIndices[2]);

                Indices.Add(Face.FaceIndices[1]);
                Indices.Add(Face.FaceIndices[2]);
                Indices.Add(Face.FaceIndices[3]);

                Indices.Add(Face.FaceIndices[2]);
                Indices.Add(Face.FaceIndices[3]);
                Indices.Add(Face.FaceIndices[0]);

                Indices.Add(Face.FaceIndices[0]);
                Indices.Add(Face.FaceIndices[1]);
                Indices.Add(Face.FaceIndices[3]);
            }

            return Indices;
        }
    }

    class BoundingBox
    {
        public Vector3 XAxis;
        public Vector3 YAxis;
        public Vector3 ZAxis;
        public Vector3 Center;
        public Vector3 HalfSize;
        public Vector3 MinPoint;
        public Vector3 MaxPoint;
        public Mesh BoundingMesh;
        public Mesh? ReferenceMesh;
        public Matrix4 Transformation;

        public BoundingBox()
        {
            Center = Vector3.Zero;
            HalfSize = Vector3.Zero;
            BoundingMesh = new Mesh();
            XAxis = new Vector3(1, 0, 0);
            YAxis = new Vector3(0, 1, 0);
            ZAxis = new Vector3(0, 0, 1);
            Transformation = Matrix4.Identity;
            MinPoint = Vector3.PositiveInfinity;
            MaxPoint = Vector3.NegativeInfinity;

            for (int i = 0; i < 8; i++)
            {
                BoundingMesh.Positions.Add(Vector3.Zero);
            }

            BoundingMesh.Indices.AddRange(BoundingFace.MinX.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MaxX.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MinY.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MaxY.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MinZ.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MaxZ.GenerateIndices());
        }

        public void Reset()
        {
            Center = Vector3.Zero;
            HalfSize = Vector3.Zero;
            BoundingMesh = new Mesh();
            XAxis = new Vector3(1, 0, 0);
            YAxis = new Vector3(0, 1, 0);
            ZAxis = new Vector3(0, 0, 1);
            Transformation = Matrix4.Identity;
            MinPoint = Vector3.PositiveInfinity;
            MaxPoint = Vector3.NegativeInfinity;

            for (int i = 0; i < 8; i++)
            {
                BoundingMesh.Positions.Add(Vector3.Zero);
            }

            BoundingMesh.Indices.AddRange(BoundingFace.MinX.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MaxX.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MinY.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MaxY.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MinZ.GenerateIndices());
            BoundingMesh.Indices.AddRange(BoundingFace.MaxZ.GenerateIndices());
        }

        public void FitMesh(Mesh Mesh)
        {
            ReferenceMesh = Mesh;
            foreach (var Position in Mesh.Positions)
            {
                if (Position.X < MinPoint.X)
                {
                    MinPoint.X = Position.X;
                }
                if (Position.Y < MinPoint.Y)
                {
                    MinPoint.Y = Position.Y;
                }
                if (Position.Z < MinPoint.Z)
                {
                    MinPoint.Z = Position.Z;
                }

                if (Position.X > MaxPoint.X)
                {
                    MaxPoint.X = Position.X;
                }
                if (Position.Y > MaxPoint.Y)
                {
                    MaxPoint.Y = Position.Y;
                }
                if (Position.Z > MaxPoint.Z)
                {
                    MaxPoint.Z = Position.Z;
                }
            }
            RefreshCenter();
            RefreshBoundingMesh();
        }

        public void RecalculateFromMesh(Mesh Mesh)
        {
            Reset();
            FitMesh(Mesh);
        }

        public void SetTransformation(Matrix4 Transformation)
        {
            this.Transformation = Transformation;
        }

        public BoundingBox GetTransformedAxisAlignedBoundingBox()
        {
            BoundingBox TransformedBB = new BoundingBox();
            Mesh TransformedMesh = new Mesh();

            if (ReferenceMesh != null)
            {
                foreach (var Position in ReferenceMesh.Positions)
                {
                    TransformedMesh.Positions.Add((new Vector4(Position, 1) * Transformation).Xyz);
                }
            }
            else
            {
                foreach (var Position in BoundingMesh.Positions)
                {
                    TransformedMesh.Positions.Add((new Vector4(Position, 1) * Transformation).Xyz);
                }
            }

            TransformedBB.FitMesh(TransformedMesh);

            return TransformedBB;
        }

        public void RefreshCenter()
        {
            HalfSize = ((MaxPoint - MinPoint) / 2);
            Center = MinPoint + HalfSize;
        }

        public void RefreshBoundingMesh()
        {
            BoundingMesh.Positions.Clear();
            BoundingMesh.Positions.Add(new Vector3(MinPoint.X, MinPoint.Y, MinPoint.Z));
            BoundingMesh.Positions.Add(new Vector3(MaxPoint.X, MinPoint.Y, MinPoint.Z));
            BoundingMesh.Positions.Add(new Vector3(MinPoint.X, MaxPoint.Y, MinPoint.Z));
            BoundingMesh.Positions.Add(new Vector3(MinPoint.X, MinPoint.Y, MaxPoint.Z));
            BoundingMesh.Positions.Add(new Vector3(MaxPoint.X, MaxPoint.Y, MinPoint.Z));
            BoundingMesh.Positions.Add(new Vector3(MaxPoint.X, MinPoint.Y, MaxPoint.Z));
            BoundingMesh.Positions.Add(new Vector3(MinPoint.X, MaxPoint.Y, MaxPoint.Z));
            BoundingMesh.Positions.Add(new Vector3(MaxPoint.X, MaxPoint.Y, MaxPoint.Z));
        }

        public bool IsCollidingAxisAligned(BoundingBox Other)
        {
            return (MinPoint.X <= Other.MaxPoint.X && MaxPoint.X >= Other.MinPoint.X) &&
                   (MinPoint.Y <= Other.MaxPoint.Y && MaxPoint.Y >= Other.MinPoint.Y) &&
                   (MinPoint.Z <= Other.MaxPoint.Z && MaxPoint.Z >= Other.MinPoint.Z);
        }

        public List<Vector2> GetFace(BoundingFace Face) 
        {
            List<Vector2> Vertices = new List<Vector2>();

            foreach (var Index in Face.FaceIndices) 
            {
                if (Face.FacePivotIndex == 0)
                {
                    Vertices.Add(BoundingMesh.Positions[(int)Index].Yz);
                }
                else if (Face.FacePivotIndex == 1)
                {
                    Vertices.Add(BoundingMesh.Positions[(int)Index].Xz);
                }
                else 
                {
                    Vertices.Add(BoundingMesh.Positions[(int)Index].Xy);
                }
            }

            return Vertices;
        }

        public void Render()
        {
            BoundingMesh.ReInitMesh();
            Window.ShaderLine.Use();
            Window.ShaderLine.SetMatrix4("Model", Transformation);
            GL.BindVertexArray(BoundingMesh.Buffers.VertexArray);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.DrawElements(PrimitiveType.Triangles, BoundingMesh.Indices.Count, DrawElementsType.UnsignedInt, 0);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public static float FaceIntersectionRatioAxisAligned(BoundingBox BoundingBoxA, BoundingFace FaceA, BoundingBox BoundingBoxB, BoundingFace FaceB)
        {
            List<Vector2> FaceAVertices = BoundingBoxA.GetFace(FaceA);
            List<Vector2> FaceBVertices = BoundingBoxB.GetFace(FaceB);
            // Min @0, Max @2

            Vector2 FaceAMin = FaceAVertices[0];
            Vector2 FaceAMax = FaceAVertices[2];

            Vector2 FaceBMin = FaceBVertices[0];
            Vector2 FaceBMax = FaceBVertices[2];

            Vector2 IntersectionMin = new Vector2(
                    Math.Max(FaceAMin.X, FaceBMin.X),
                    Math.Max(FaceAMin.Y, FaceBMin.Y)
                );
            Vector2 IntersectionMax = new Vector2(
                    Math.Min(FaceAMax.X, FaceBMax.X),
                    Math.Min(FaceAMax.Y, FaceBMax.Y)
                );

            if (IntersectionMax.X < IntersectionMin.X || IntersectionMax.Y < IntersectionMin.Y) 
            {
                return 0;
            }

            Vector2 FaceASize = FaceAMax - FaceAMin;
            Vector2 FaceBSize = FaceBMax - FaceBMin;

            Vector2 IntersectionSize = IntersectionMax - IntersectionMin;

            float FaceAArea = FaceASize.X * FaceASize.Y;
            float FaceBArea = FaceBSize.X * FaceBSize.Y;

            float IntersectionArea = IntersectionSize.X * IntersectionSize.Y;

            return Math.Max(IntersectionArea/FaceBArea , IntersectionArea / FaceAArea);
        }
    }
}
