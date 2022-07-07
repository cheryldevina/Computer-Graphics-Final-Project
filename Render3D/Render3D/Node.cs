using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Render3D
{
    class Node
    {
        public String Name;
        public Node? Parent;
        public Model ParentModel;
        public List<Node> Children;
        public List<int> MeshIndices;
        public Vector3 ScaleVector;
        public Vector3 PositionVector;
        public Quaternion RotationQuatertion;
        public Matrix4 OriginMatrix;
        public Matrix4 TransformMatrix;
        public Matrix4 ParentInverseMatrix;

        public Node(Model ParentModel, String Name)
        {
            this.Name = Name;
            this.ParentModel = ParentModel;
            ParentModel.Nodes.Add(Name, this);
            Children = new List<Node>();
            MeshIndices = new List<int>();
            OriginMatrix = Matrix4.Identity;
            TransformMatrix = Matrix4.Identity;
            ParentInverseMatrix = Matrix4.Identity;
            DecomposeTransformMatrix();
        }

        public Node AddChild(Node Child)
        {
            Child.Parent = this;

            RefreshTransformMatrix();
            Child.ParentInverseMatrix = this.TransformMatrix.Inverted();

            Children.Add(Child);

            return this;
        }

        public Node(Assimp.Node Node, Model ParentModel)
        {
            Name = Node.Name;
            this.ParentModel = ParentModel;
            ParentModel.Nodes.Add(Name, this);
            MeshIndices = new List<int>();
            OriginMatrix = Matrix4.Identity;

            foreach (var MeshIndex in Node.MeshIndices)
            {
                MeshIndices.Add(MeshIndex);
            }

            Assimp.Matrix4x4 PS = Node.Transform;

            TransformMatrix = new Matrix4(
                new Vector4(PS.A1, PS.A2, PS.A3, PS.A4),
                new Vector4(PS.B1, PS.B2, PS.B3, PS.B4), 
                new Vector4(PS.C1, PS.C2, PS.C3, PS.C4),
                new Vector4(PS.D1, PS.D2, PS.D3, PS.D4));

            TransformMatrix.Transpose();

            DecomposeTransformMatrix();

            TransformMatrix = Matrix4.Identity;

            RefreshTransformMatrix();

            Children = new List<Node>();
            foreach (var Child in Node.Children)
            {
                Node NewChild = new Node(Child, ParentModel);
                NewChild.Parent = this;

                Console.WriteLine($"Loading Node: {Name} -> {Child.Name}");

                Children.Add(NewChild);
            }
        }

        public void DecomposeTransformMatrix()
        {
            ScaleVector = TransformMatrix.ExtractScale();
            PositionVector = TransformMatrix.ExtractTranslation();
            RotationQuatertion = TransformMatrix.ExtractRotation();
        }

        public Node Scale(float X, float Y, float Z)
        {
            TransformMatrix *= Matrix4.CreateScale(new Vector3(X, Y, Z));

            DecomposeTransformMatrix();

            return this;
        }

        public Node Rotate(float X, float Y, float Z)
        {
            Vector3 TempPos = PositionVector;

            TransformMatrix *= Matrix4.CreateRotationX(X);
            TransformMatrix *= Matrix4.CreateRotationY(Y);
            TransformMatrix *= Matrix4.CreateRotationZ(Z);

            DecomposeTransformMatrix();

            PositionVector = TempPos;

            RefreshTransformMatrix();

            return this; ;
        }

        public Node Translate(float X, float Y, float Z)
        {
            TransformMatrix *= Matrix4.CreateTranslation(new Vector3(X, Y, Z));

            DecomposeTransformMatrix();

            return this;
        }

        public Node SetScale(float X, float Y, float Z)
        {
            ScaleVector.X = X;
            ScaleVector.Y = Y;
            ScaleVector.Z = Z;

            RefreshTransformMatrix();

            return this;
        }

        public Node SetRotation(float X, float Y, float Z, float W)
        {
            this.RotationQuatertion.X = X;
            this.RotationQuatertion.Y = Y;
            this.RotationQuatertion.Z = Z;
            this.RotationQuatertion.W = W;

            RefreshTransformMatrix();

            return this;
        }

        public Node SetPosition(float X, float Y, float Z)
        {
            this.PositionVector.X = X;
            this.PositionVector.Y = Y;
            this.PositionVector.Z = Z;

            RefreshTransformMatrix();

            return this;
        }

        public void RefreshTransformMatrix()
        {
            TransformMatrix = Matrix4.Identity;
            TransformMatrix *= Matrix4.CreateScale(ScaleVector);
            TransformMatrix *= Matrix4.CreateFromQuaternion(RotationQuatertion);
            TransformMatrix *= Matrix4.CreateTranslation(PositionVector);
        }

        public void Render(Matrix4? ParentTransform = null)
        {
            if (ParentTransform == null) 
            {
                ParentTransform = Matrix4.Identity;
            }

            Matrix4 AppliedTransform;
            RefreshTransformMatrix();
            if (OriginMatrix != Matrix4.Identity)
            {
                AppliedTransform = (Matrix4)(OriginMatrix.Inverted() * TransformMatrix * ParentInverseMatrix * OriginMatrix * ParentTransform);
            }
            else 
            {
                AppliedTransform = (Matrix4)(TransformMatrix * ParentTransform);
            }

            foreach (var MeshIndex in MeshIndices)
            {
                Mesh Mesh = ParentModel.Meshes[MeshIndex];

                Material Material = ParentModel.Materials[Mesh.MaterialIndex];
                Window.Shader.SetInt("ObjectMaterial.DiffuseMap", 1);
                Window.Shader.SetInt("ObjectMaterial.SpecularMap", 2);
                Window.Shader.SetInt("ObjectMaterial.NormalMap", 3);
                Window.Shader.SetInt("ObjectMaterial.DisplacementMap", 4);
                Window.Shader.SetInt("ObjectMaterial.AlphaMap", 5);
                Window.Shader.SetInt("ObjectMaterial.EmissiveMap", 6);
                Window.Shader.SetInt("ObjectMaterial.MetalnessMap", 7);
                Window.Shader.SetInt("ObjectMaterial.RoughnessMap", 8);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.DiffuseMap].TextureHandle);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.SpecularMap].TextureHandle);
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.NormalMap].TextureHandle);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.DisplacementMap].TextureHandle); 
                GL.ActiveTexture(TextureUnit.Texture5);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.OpacityMap].TextureHandle);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.EmissiveMap].TextureHandle);
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.MetalnessMap].TextureHandle);
                GL.ActiveTexture(TextureUnit.Texture8);
                GL.BindTexture(TextureTarget.Texture2D, ParentModel.Textures[Material.RoughnessMap].TextureHandle);
                Window.Shader.SetMatrix4("Model", AppliedTransform);
                Window.Shader.SetVector3("ObjectMaterial.Ambient", Material.Ambient);
                Window.Shader.SetVector3("ObjectMaterial.Diffuse", Material.Diffuse);
                Window.Shader.SetVector3("ObjectMaterial.Specular", Material.Specular);
                Window.Shader.SetFloat("ObjectMaterial.Shininess", Material.Shininess);
                Window.Shader.SetFloat("ObjectMaterial.Alpha", Material.Alpha);
                Window.Shader.SetFloat("ObjectMaterial.Metalness", Material.Metalness);
                Window.Shader.SetFloat("ObjectMaterial.Roughness", Material.Roughness);
                if (Material.DisplacementMap != "BLACK_DEFAULT_MAP") 
                {
                    Window.Shader.SetFloat("ObjectMaterial.DispScale", Material.DisplacementScale);
                }
                Window.Shader.SetInt("ObjectMaterial.UseDiffuseMap", Material.UseDiffuseMap);
                Window.Shader.SetInt("ObjectMaterial.UseNormalMap", Material.UseNormalMap);
                Window.Shader.SetInt("ObjectMaterial.IsPBR", Material.IsPBR);
                GL.BindVertexArray(Mesh.Buffers.VertexArray);
                GL.DrawElements(PrimitiveType.Triangles, Mesh.Indices.Count, DrawElementsType.UnsignedInt, 0);

                if (Mesh.BoundingBox != null) 
                { 
                    Mesh.BoundingBox.SetTransformation(AppliedTransform);

                    if (ParentModel.CalculateMinMax)
                    {
                        BoundingBox TransformedBoundingBox = Mesh.BoundingBox.GetTransformedAxisAlignedBoundingBox();

                        foreach (var Position in TransformedBoundingBox.BoundingMesh.Positions)
                        {
                            if (Position.X < ParentModel.BoundingBox.MinPoint.X)
                            {
                                ParentModel.BoundingBox.MinPoint.X = Position.X;
                            }
                            if (Position.X > ParentModel.BoundingBox.MaxPoint.X)
                            {
                                ParentModel.BoundingBox.MaxPoint.X = Position.X;
                            }

                            if (Position.Y < ParentModel.BoundingBox.MinPoint.Y)
                            {
                                ParentModel.BoundingBox.MinPoint.Y = Position.Y;
                            }
                            if (Position.Y > ParentModel.BoundingBox.MaxPoint.Y)
                            {
                                ParentModel.BoundingBox.MaxPoint.Y = Position.Y;
                            }

                            if (Position.Z < ParentModel.BoundingBox.MinPoint.Z)
                            {
                                ParentModel.BoundingBox.MinPoint.Z = Position.Z;
                            }
                            if (Position.Z > ParentModel.BoundingBox.MaxPoint.Z)
                            {
                                ParentModel.BoundingBox.MaxPoint.Z = Position.Z;
                            }
                        }
                    }

                    if (Mesh.Collision)
                    {
                        Window.PlayerController.DoCollider(Mesh);
                    }

                    if (Window.DrawBoundingBox && Mesh.Collision)
                    {
                        Mesh.BoundingBox.GetTransformedAxisAlignedBoundingBox().Render();
                    }
                }

                GL.BindVertexArray(0);
            }

            foreach (var Child in Children)
            {
                Child.Render(AppliedTransform);
            }
        }

        public void SetOrigin(float X, float Y, float Z)
        {
            OriginMatrix = Matrix4.CreateTranslation(X, Y, Z);
        }

        public Dictionary<int, Mesh> GetMeshes(Dictionary<int,Mesh> Meshes)
        {
            foreach (var MeshIndex in MeshIndices) 
            {
                if (!Meshes.ContainsKey(MeshIndex)) 
                {
                    Meshes.Add(MeshIndex,ParentModel.Meshes[MeshIndex]);
                }
            }

            foreach (var Child in Children)
            {
                Meshes = Child.GetMeshes(Meshes);
            }

            return Meshes;
        }

        public List<Mesh> GetMeshes() 
        {
            Dictionary<int, Mesh> Meshes = new();
            Meshes = GetMeshes(Meshes);

            return Meshes.Values.ToList<Mesh>();
        }
    }
}