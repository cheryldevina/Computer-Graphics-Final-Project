using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Render3D
{
    public struct BufferObject
    {
        public int VertexArray = -1;
        public int Element = -1;
        public int Vertex = -1;
        public int TexCoord = -1;
        public int Normal = -1;
        public int Tangent = -1;
        public int Bitangent = -1;
        public BufferObject() { }
    }

    class Mesh
    {
        public string Name;
        public int MaterialIndex;
        public List<uint> Indices;
        public List<Vector3> Positions;
        public List<Vector2> TexCoords;
        public List<Vector3> Normals;
        public List<Vector3> Tangents;
        public List<Vector3> Bitangents;
        public BufferObject Buffers;
        public BoundingBox? BoundingBox;
        public bool Collision = false;

        public Mesh()
        {
            Name = "";
            MaterialIndex = -1;
            Indices = new List<uint>();
            Positions = new List<Vector3>();
            TexCoords = new List<Vector2>();
            Normals = new List<Vector3>();
            Tangents = new List<Vector3>();
            Bitangents = new List<Vector3>();
            Buffers = new BufferObject();
            BoundingBox = null;
            InitMesh();
        }

        public Mesh(Assimp.Mesh Mesh)
        {
            Name = Mesh.Name;
            Indices = new List<uint>();
            Positions = new List<Vector3>();
            TexCoords = new List<Vector2>();
            Normals = new List<Vector3>();
            Tangents = new List<Vector3>();
            Bitangents = new List<Vector3>();
            BoundingBox = new BoundingBox();
            MaterialIndex = Mesh.MaterialIndex;

            foreach (var Face in Mesh.Faces)
            {
                foreach (var Index in Face.Indices) 
                { 
                    Indices.Add((uint)Index);
                }
            }

            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                Positions.Add(new Vector3(Mesh.Vertices[i].X, Mesh.Vertices[i].Y, Mesh.Vertices[i].Z));
                Normals.Add(new Vector3(Mesh.Normals[i].X, Mesh.Normals[i].Y, Mesh.Normals[i].Z));
                if (Mesh.HasTextureCoords(0))
                {
                    TexCoords.Add(new Vector2(Mesh.TextureCoordinateChannels[0][i].X, Mesh.TextureCoordinateChannels[0][i].Y));
                }
                else 
                {
                    TexCoords.Add(Vector2.Zero);
                }

                if (Mesh.HasTangentBasis)
                {
                    Tangents.Add(new Vector3(Mesh.Tangents[i].X, Mesh.Tangents[i].Y, Mesh.Tangents[i].Z));
                    Bitangents.Add(new Vector3(Mesh.BiTangents[i].X, Mesh.BiTangents[i].Y, Mesh.BiTangents[i].Z));
                }
            }

            if (!Mesh.HasTangentBasis && TexCoords.Count > 0)
            {
                GenerateTangentsBitangents();
            }

            BoundingBox.FitMesh(this);

            InitMesh();
        }

        public void SetCollision(bool Status)
        {
            Console.WriteLine("Mesh: " + Name + " collision set to " + Status.ToString());
            Collision = Status;
        }

        public void GenerateTangentsBitangents()
        {
            Tangents = new List<Vector3>(Normals);
            Bitangents = new List<Vector3>(Normals);

            if (TexCoords.Count != Positions.Count)
            {
                GenerateTexCoords();
            }

            for (int i = 2; i < Indices.Count; i += 3)
            {
                Vector3 EdgeA = Positions[(int)Indices[i - 1]] - Positions[(int)Indices[i - 2]];
                Vector3 EdgeB = Positions[(int)Indices[i]] - Positions[(int)Indices[i - 2]];
                Vector2 DeltaUVA = TexCoords[(int)Indices[i - 1]] - TexCoords[(int)Indices[i - 2]];
                Vector2 DeltaUVB = TexCoords[(int)Indices[i]] - TexCoords[(int)Indices[i - 2]];

                float f = 1.0f / (DeltaUVA.X * DeltaUVB.Y - DeltaUVB.X * DeltaUVA.Y);

                Vector3 Tangent = Vector3.Zero;
                Vector3 Bitangent = Vector3.Zero;

                Tangent.X = f * (DeltaUVB.Y * EdgeA.X - DeltaUVA.Y * EdgeB.X);
                Tangent.Y = f * (DeltaUVB.Y * EdgeA.Y - DeltaUVA.Y * EdgeB.Y);
                Tangent.Z = f * (DeltaUVB.Y * EdgeA.Z - DeltaUVA.Y * EdgeB.Z);

                Bitangent.X = f * (-DeltaUVB.X * EdgeA.X + DeltaUVA.X * EdgeB.X);
                Bitangent.Y = f * (-DeltaUVB.X * EdgeA.Y + DeltaUVA.X * EdgeB.Y);
                Bitangent.Z = f * (-DeltaUVB.X * EdgeA.Z + DeltaUVA.X * EdgeB.Z);

                Tangents[(int)Indices[i - 2]] = Tangent;
                Tangents[(int)Indices[i - 1]] = Tangent;
                Tangents[(int)Indices[i]] = Tangent;

                Bitangents[(int)Indices[i - 2]] = Bitangent;
                Bitangents[(int)Indices[i - 1]] = Bitangent;
                Bitangents[(int)Indices[i]] = Bitangent;
            }
        }

        public void GenerateTexCoords()
        {
            TexCoords = new List<Vector2>();
            for (int i = 0; i < Positions.Count; i++)
            {
                TexCoords.Add(Vector2.Zero);
            }
        }

        public void InitMesh()
        {
            Buffers.Vertex = GL.GenBuffer();
            Buffers.TexCoord = GL.GenBuffer();
            Buffers.Element = GL.GenBuffer();
            Buffers.VertexArray = GL.GenVertexArray();
            Buffers.Normal = GL.GenBuffer();
            Buffers.Tangent = GL.GenBuffer();
            Buffers.Bitangent = GL.GenBuffer();

            GL.BindVertexArray(Buffers.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Vertex);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Positions.Count * Vector3.SizeInBytes,
                Positions.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.TexCoord);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                TexCoords.Count * Vector2.SizeInBytes,
                TexCoords.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Normal);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Normals.Count * Vector3.SizeInBytes,
                Normals.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Tangent);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Tangents.Count * Vector3.SizeInBytes,
                Tangents.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Bitangent);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Bitangents.Count * Vector3.SizeInBytes,
                Bitangents.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Vertex);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.TexCoord);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Normal);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Tangent);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Bitangent);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(4);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffers.Element);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                Indices.Count * sizeof(uint),
                Indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }
        public void ReInitMesh()
        {
            GL.BindVertexArray(Buffers.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Vertex);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Positions.Count * Vector3.SizeInBytes,
                Positions.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.TexCoord);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                TexCoords.Count * Vector2.SizeInBytes,
                TexCoords.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Normal);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Normals.Count * Vector3.SizeInBytes,
                Normals.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Tangent);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Tangents.Count * Vector3.SizeInBytes,
                Tangents.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Bitangent);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                Bitangents.Count * Vector3.SizeInBytes,
                Bitangents.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Vertex);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.TexCoord);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Normal);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Tangent);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Bitangent);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(4);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffers.Element);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                Indices.Count * sizeof(uint),
                Indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }
    }
}
