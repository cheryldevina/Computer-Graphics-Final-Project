using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace Render3D
{
    class Model
    {
        public int ModelIndex;
        public Node ParentNode;
        public List<Mesh> Meshes;
        public List<Material> Materials;
        public Dictionary<string, Node> Nodes;
        public Dictionary<string, Texture> Textures;
        public List<Timeline> Timelines;
        public BoundingBox BoundingBox;
        public bool CalculateMinMax;

        public Model(string Path, int ModelIndex, bool TextureYFlip = false)
        {
            AssimpContext Importer = new AssimpContext();
            Assimp.Scene Loader = Importer.ImportFile(Path, PostProcessSteps.Triangulate);
            String? Directory = System.IO.Path.GetDirectoryName(Path);
            Directory += "\\";

            Meshes = new List<Mesh>();
            Materials = new List<Material>();
            Textures = new Dictionary<string, Texture>();
            Nodes = new Dictionary<string, Node>();
            Timelines = new List<Timeline>();
            this.ModelIndex = ModelIndex;

            Textures.Add("BLACK_DEFAULT_MAP", new Texture(new List<byte>(4 * 1 * 1) { 0, 0, 0, 255 }.ToArray(), 1, 1));
            Textures.Add("WHITE_DEFAULT_MAP", new Texture(new List<byte>(4 * 1 * 1) { 255, 255, 255, 255 }.ToArray(), 1, 1));

            Console.WriteLine("\n[Meshes]");

            foreach (var Mesh in Loader.Meshes)
            {
                Console.WriteLine("Loading Mesh: " + Mesh.Name);
                Meshes.Add(new Mesh(Mesh));
            }

            Console.WriteLine("\n[Materials & Textures]");

            foreach (var Material in Loader.Materials)
            {
                Console.WriteLine("Loading Material: " + Material.Name);
                Materials.Add(new Material(Material));

                if (Material.HasTextureDiffuse)
                {
                    if (Material.TextureDiffuse.FilePath != "")
                    {
                        Console.WriteLine("Loading Diffuse Texture: " + Material.TextureDiffuse.FilePath);
                        if (!Textures.ContainsKey(Material.TextureDiffuse.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(Material.TextureDiffuse.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Console.WriteLine("Diffuse compressed!");
                                    Textures.Add(Material.TextureDiffuse.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                    Console.WriteLine(Material.TextureDiffuse.FilePath);
                                }

                            }
                            else
                            {
                                Textures.Add(Material.TextureDiffuse.FilePath, new Texture(Directory + Material.TextureDiffuse.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.HasTextureSpecular)
                {
                    if (Material.TextureSpecular.FilePath != "")
                    {
                        Console.WriteLine("Loading Specular Texture: " + Material.TextureSpecular.FilePath);
                        if (!Textures.ContainsKey(Material.TextureSpecular.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(Material.TextureSpecular.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(Material.TextureSpecular.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                    Console.WriteLine(Material.TextureSpecular.FilePath);
                                }
                            }
                            else
                            {
                                Textures.Add(Material.TextureSpecular.FilePath, new Texture(Directory + Material.TextureSpecular.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.HasTextureEmissive)
                {
                    if (Material.TextureEmissive.FilePath != "")
                    {
                        Console.WriteLine("Loading Emissive Texture: " + Material.TextureEmissive.FilePath);
                        if (!Textures.ContainsKey(Material.TextureEmissive.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(Material.TextureEmissive.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(Material.TextureEmissive.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                    Console.WriteLine(Material.TextureEmissive.FilePath);
                                }
                            }
                            else
                            {
                                Textures.Add(Material.TextureEmissive.FilePath, new Texture(Directory + Material.TextureEmissive.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.HasTextureNormal)
                {
                    if (Material.TextureNormal.FilePath != "")
                    {
                        Console.WriteLine("Loading Normal Texture: " + Material.TextureNormal.FilePath);
                        if (!Textures.ContainsKey(Material.TextureNormal.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(Material.TextureNormal.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(Material.TextureNormal.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                    Console.WriteLine(Material.TextureNormal.FilePath);
                                }
                            }
                            else
                            {
                                Textures.Add(Material.TextureNormal.FilePath, new Texture(Directory + Material.TextureNormal.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.HasTextureDisplacement)
                {
                    if (Material.TextureDisplacement.FilePath != "")
                    {
                        Console.WriteLine("Loading Displacement Texture: " + Material.TextureDisplacement.FilePath);
                        if (!Textures.ContainsKey(Material.TextureDisplacement.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(Material.TextureDisplacement.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(Material.TextureDisplacement.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                }
                            }
                            else
                            {
                                Textures.Add(Material.TextureDisplacement.FilePath, new Texture(Directory + Material.TextureDisplacement.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.HasTextureOpacity)
                {
                    if (Material.TextureOpacity.FilePath != "")
                    {
                        Console.WriteLine("Loading Opacity Texture: " + Material.TextureOpacity.FilePath);
                        if (!Textures.ContainsKey(Material.TextureOpacity.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(Material.TextureOpacity.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(Material.TextureOpacity.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                }
                            }
                            else
                            {
                                Textures.Add(Material.TextureOpacity.FilePath, new Texture(Directory + Material.TextureOpacity.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.GetMaterialTexture(Assimp.TextureType.Metalness, 0, out Assimp.TextureSlot MetalnessSlot))
                {
                    if (MetalnessSlot.FilePath != "")
                    {
                        Console.WriteLine("Loading Metalness Texture: " + MetalnessSlot.FilePath);
                        if (!Textures.ContainsKey(MetalnessSlot.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(MetalnessSlot.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(MetalnessSlot.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                }
                            }
                            else
                            {
                                Textures.Add(MetalnessSlot.FilePath, new Texture(Directory + MetalnessSlot.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.GetMaterialTexture(Assimp.TextureType.Roughness, 0, out Assimp.TextureSlot RoughnessSlot))
                {
                    if (RoughnessSlot.FilePath != "")
                    {
                        Console.WriteLine("Loading Roughness Texture: " + RoughnessSlot.FilePath);
                        if (!Textures.ContainsKey(RoughnessSlot.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(RoughnessSlot.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(RoughnessSlot.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                }
                            }
                            else
                            {
                                Textures.Add(RoughnessSlot.FilePath, new Texture(Directory + RoughnessSlot.FilePath, TextureYFlip));
                            }
                        }
                    }
                }

                if (Material.GetMaterialTexture(Assimp.TextureType.Unknown, 0, out Assimp.TextureSlot MetalnessRoughnessSlot))
                {
                    if (MetalnessRoughnessSlot.FilePath != "")
                    {
                        Console.WriteLine("Loading MetallicRoughnessSlot Texture: " + MetalnessRoughnessSlot.FilePath);
                        if (!Textures.ContainsKey(MetalnessRoughnessSlot.FilePath + "_Metalness") && !Textures.ContainsKey(MetalnessRoughnessSlot.FilePath + "_Roughness"))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(MetalnessRoughnessSlot.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    List<Texture> SplitTextures = Texture.GenerateMetalRoughSplit(EmbeddedTexture.CompressedData, TextureYFlip);
                                    Textures.Add(MetalnessRoughnessSlot.FilePath + "_Metalness", SplitTextures[0]);
                                    Textures.Add(MetalnessRoughnessSlot.FilePath + "_Roughness", SplitTextures[1]);
                                }
                            }
                            else
                            {
                                List<Texture> SplitTextures = Texture.GenerateMetalRoughSplit(Directory + MetalnessRoughnessSlot.FilePath, TextureYFlip);
                                Textures.Add(MetalnessRoughnessSlot.FilePath + "_Metalness", SplitTextures[0]);
                                Textures.Add(MetalnessRoughnessSlot.FilePath + "_Roughness", SplitTextures[1]);
                            }
                        }

                    }
                }

                if (Material.GetMaterialTexture(Assimp.TextureType.BaseColor, 0, out Assimp.TextureSlot BaseColorSlot))
                {
                    if (BaseColorSlot.FilePath != "")
                    {
                        Console.WriteLine("Loading Albedo Texture: " + BaseColorSlot.FilePath);
                        if (!Textures.ContainsKey(BaseColorSlot.FilePath))
                        {
                            EmbeddedTexture EmbeddedTexture = Loader.GetEmbeddedTexture(BaseColorSlot.FilePath);
                            if (EmbeddedTexture != null)
                            {
                                if (EmbeddedTexture.IsCompressed)
                                {
                                    Textures.Add(BaseColorSlot.FilePath, new Texture(EmbeddedTexture.CompressedData, TextureYFlip));
                                }
                            }
                            else
                            {
                                Textures.Add(BaseColorSlot.FilePath, new Texture(Directory + BaseColorSlot.FilePath, TextureYFlip));
                            }
                        }
                    }
                }
            }

            Console.WriteLine("\n[Nodes]");
            ParentNode = new Node(Loader.RootNode, this);

            float MaxAnimationTime = 0;

            Console.WriteLine("\n[Animations]\n");

            foreach (var Animation in Loader.Animations)
            {
                if (Animation.DurationInTicks / Animation.TicksPerSecond > MaxAnimationTime)
                {
                    MaxAnimationTime = (float)Animation.DurationInTicks / (float)Animation.TicksPerSecond;
                }
            }

            foreach (var Animation in Loader.Animations)
            {
                foreach (var Node in Animation.NodeAnimationChannels)
                {
                    Console.WriteLine("Loading Animation: " + Animation.Name + " -> " + Node.NodeName);
                    Timelines.Add(new Timeline(Nodes[Node.NodeName], (float)Animation.TicksPerSecond, MaxAnimationTime * (float)Animation.TicksPerSecond, Node.ScalingKeys, Node.PositionKeys, Node.RotationKeys));
                }
            }

            Console.WriteLine("\n[Lights]");

            if (Loader.HasLights)
            {
                foreach (var Light in Loader.Lights)
                {
                    Node? TempParentNode = Nodes[Light.Name].Parent;

                    if (TempParentNode != null)
                    {
                        Console.WriteLine(String.Format("Name: {0}", TempParentNode.Name));
                        Vector3 InitialPosition = new Vector3(Light.Position.X, Light.Position.Y, Light.Position.Z);
                        Vector3 InitDir = new Vector3(0, 1, 0);
                        Matrix4 PS = TempParentNode.TransformMatrix;

                        Matrix4 ParentTransform = new Matrix4(
                            new Vector4(PS.M11, PS.M12, PS.M13, PS.M14),
                            new Vector4(PS.M21, PS.M22, PS.M23, PS.M24),
                            new Vector4(PS.M31, PS.M32, PS.M33, PS.M34),
                            new Vector4(PS.M41, PS.M42, PS.M43, PS.M44));

                        Vector4 CalculatePos = new Vector4(InitialPosition, 1) * ParentTransform;
                        Vector4 CalculateDir = new Vector4(InitDir, 1) * ParentTransform;

                        Vector3 Position = CalculatePos.Xyz;
                        Vector3 Direction = Position - CalculateDir.Xyz;

                        Vector3 Ambient = new Vector3(
                            Light.ColorAmbient.R,
                            Light.ColorAmbient.G,
                            Light.ColorAmbient.B);
                        Vector3 Diffuse = new Vector3(
                            Light.ColorDiffuse.R,
                            Light.ColorDiffuse.G,
                            Light.ColorDiffuse.B);
                        Vector3 Specular = new Vector3(
                            Light.ColorSpecular.R,
                            Light.ColorSpecular.G,
                            Light.ColorSpecular.B);

                        Console.WriteLine(Ambient);
                        Console.WriteLine(Diffuse);
                        Console.WriteLine(Specular);

                        if (Light.LightType == LightSourceType.Directional)
                        {
                            Window.Lights.Add(
                                Render3D.Light.GenerateDirLight(
                                    TempParentNode.Name,
                                    Light.Name,
                                    ModelIndex,
                                    ParentTransform,
                                    Ambient,
                                    Diffuse,
                                    Specular,
                                    Direction,
                                    InitialPosition));
                        }

                        if (Light.LightType == LightSourceType.Point)
                        {
                            Window.Lights.Add(
                                Render3D.Light.GeneratePointLight(
                                    TempParentNode.Name,
                                    Light.Name,
                                    ModelIndex,
                                    ParentTransform,
                                    Ambient,
                                    Diffuse,
                                    Specular,
                                    Position,
                                    InitialPosition));
                        }

                        if (Light.LightType == LightSourceType.Spot)
                        {
                            float InnerCutOff = Light.AngleInnerCone;
                            float OuterCutOff = Light.AngleOuterCone;
                            Window.Lights.Add(
                                Render3D.Light.GenerateSpotlight(
                                    TempParentNode.Name,
                                    Light.Name,
                                    ModelIndex,
                                    ParentTransform,
                                    Ambient,
                                    Diffuse,
                                    Specular,
                                    Position,
                                    Direction,
                                    InitialPosition,
                                    1 - (InnerCutOff / 2.0f),
                                    1 - (OuterCutOff / 2.0f)));
                        }
                    }
                }
            }

            BoundingBox = new BoundingBox();
            Console.WriteLine("\nLoading Complete!\n");
        }

        public Mesh? FindMesh(string Name)
        {
            foreach (var Mesh in Meshes)
            {
                if (Mesh.Name == Name)
                {
                    return Mesh;
                }
            }

            return null;
        }

        public Node? FindNode(string Name)
        {
            foreach (var Node in Nodes)
            {
                if (Node.Key == Name)
                {
                    return Node.Value;
                }
            }

            return null;
        }

        public void Render()
        {
            BoundingBox.Reset();
            ParentNode.Render();
            BoundingBox.RefreshCenter();
            BoundingBox.RefreshBoundingMesh();
        }
    }
}



