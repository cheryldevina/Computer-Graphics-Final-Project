using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using LearnOpenTK.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Render3D
{

    public static class ExtensionMethods
    {
        public static float Rad(this float degree)
        {
            return (float)((degree * Math.PI) / 180);
        }

    }

    static class Constants
    {
        public const string ShadersPath = "../../../Shaders/";
        public const string ObjectsPath = "../../../Assets/";
        public const int GlobalLighting = 0;
        public const int MaxLight = 30;
        public const float AmbientValue = 0.1f;
        public const float LightScale = 0.1f;
        public const float SpeedyGonzalez = 5.5f;
        public const float Sensitivity = 0.05f;
        public const float YGround = -0.65f;
        public const float Gravity = 5f;
        public const float JumpHeight = 1f;
        public const float TPDistance = 4f;
    }

    class Window : GameWindow
    {
        public static Shader Shader;
        public static Shader ShaderLine;
        public static List<Light> Lights;
        public static Camera Camera;
        public List<Model> ModelList;
        public static PlayerController PlayerController;
        public static bool DrawBoundingBox = false;

        public Window(GameWindowSettings GameWindowSettings, NativeWindowSettings NativeWindowSettings) : base(GameWindowSettings, NativeWindowSettings)
        {
            ModelList = new List<Model>();
            Lights = new List<Light>();
            Shader = new Shader(Constants.ShadersPath + "Shader.vert", Constants.ShadersPath + "Shader.frag");
            ShaderLine = new Shader(Constants.ShadersPath + "ShaderLine.vert", Constants.ShadersPath + "ShaderLine.frag");
            Camera = new Camera(60 , (float)Size.X / (float)Size.Y, 0.01f, 10000f);

            ModelList.Add(new Model(Constants.ObjectsPath + "NewVersion.glb", ModelList.Count, true));

            Model CharacterModel = new Model(Constants.ObjectsPath + "PigWalkSpiderman.glb", ModelList.Count, true);
            CharacterModel.ParentNode.SetScale(0.5f, 0.5f, 0.5f);
            PlayerController = new PlayerController(CharacterModel, Camera);

            // enable "solid" / "collision"
            ModelList[0].FindMesh("mesh_Group51674_12268Mountain3")?.SetCollision(true);
            ModelList[0].FindNode("Wolf")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("Wolf.001")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("Yellow_Terrain")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("Barn")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("House")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("Well")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("HB1")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("HB2")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("HB3")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("F1")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("F2")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("F3")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("F4")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("F5")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("F6")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("Steve")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("mobil")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("Kincir")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("SC1")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            ModelList[0].FindNode("SC2")?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));



            //Window.Lights.Add(Light.GenerateSpotlight(
            //    "Barn", "test", 0, Matrix4.Identity,
            //    Vector3.One, Vector3.One, Vector3.One,
            //    Vector3.Zero, new Vector3(0, -1, 0), Vector3.Zero,
            //    0.9f, 0.8f));

            //ModelList[0].FindNode(ModelList[0].ParentNode.Name)?.GetMeshes().ForEach(Mesh => Mesh.SetCollision(true));
            //ModelList[0].Meshes.ForEach(Mesh => Mesh.SetCollision(true));
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0, 220, 255, 255);
            GL.Enable(EnableCap.Multisample);
            GL.Hint(HintTarget.MultisampleFilterHintNv, HintMode.Fastest);
            //GL.Hint(HintTarget.MultisampleFilterHintNv, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.FramebufferSrgb);

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs Args)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            Camera.AspectRatio = (float)Size.X / (float)Size.Y;
            Camera.RefreshProjectionMatrix();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Shader.SetMatrix4("Projection", Camera.ProjectionMatrix);
            Shader.SetMatrix4("View", Camera.ViewMatrix);
            Shader.SetVector3("ViewPos", Camera.Position);

            ShaderLine.SetMatrix4("Projection", Camera.ProjectionMatrix);
            ShaderLine.SetMatrix4("View", Camera.ViewMatrix);

            Shader.SetInt("GlobalLighting", Constants.GlobalLighting);

            foreach (var Light in Window.Lights)
            {
                Light.RefreshLightMatrix(ModelList[Light.ModelIndex].Nodes[Light.ParentName]);
            }
            Shader.SetInt("LightCount", (Lights.Count() < Constants.MaxLight ? Lights.Count() : Constants.MaxLight));
            int LightCounter = 0;
            foreach (var Light in Lights)
            {
                Shader.SetInt(String.Format("LightArray[{0}].Type", LightCounter), Light.LightType);
                // Positions
                Shader.SetVector3(String.Format("LightArray[{0}].Position", LightCounter), Light.Position);
                Shader.SetVector3(String.Format("LightArray[{0}].Direction", LightCounter), Light.Direction);
                // Light color
                Shader.SetVector3(String.Format("LightArray[{0}].Ambient", LightCounter), Light.Ambient);
                Shader.SetVector3(String.Format("LightArray[{0}].Diffuse", LightCounter), Light.Diffuse);
                Shader.SetVector3(String.Format("LightArray[{0}].Specular", LightCounter), Light.Specular);
                // Spotlight
                Shader.SetFloat(String.Format("LightArray[{0}].InnerCutOff", LightCounter), Light.InnerCutOff);
                Shader.SetFloat(String.Format("LightArray[{0}].OuterCutOff", LightCounter), Light.OuterCutOff);

                Shader.SetFloat(String.Format("LightArray[{0}].Constant", LightCounter), Light.Constant);
                Shader.SetFloat(String.Format("LightArray[{0}].Linear", LightCounter), Light.Linear);
                Shader.SetFloat(String.Format("LightArray[{0}].Quadratic", LightCounter), Light.Quadratic);
                Shader.SetFloat(String.Format("LightArray[{0}].FarPlane", LightCounter), Light.FarPlane);

                LightCounter += 1;
            }

            PlayerController.Character.Render();
            if (Window.DrawBoundingBox) 
            {
                PlayerController.Character.BoundingBox.Render();
            }

            foreach (var Model in ModelList)
            {
                Model.Render();
            }

            SwapBuffers();
            base.OnRenderFrame(Args);
        }

        protected override void OnResize(ResizeEventArgs Args)
        {
            if (Size.X > 0 && Size.Y > 0)
            {
                GL.Viewport(0, 0, Size.X, Size.Y);
                Camera.AspectRatio = (float)Size.X / (float)Size.Y;
                Camera.RefreshProjectionMatrix();
            }
            base.OnResize(Args);
        }

        protected override void OnUpdateFrame(FrameEventArgs Args)
        {
            Animator.Ticker.Update((float)Args.Time * 100);

            this.CursorVisible = false;

            PlayerController.Update(Args, this);

            this.MousePosition = new Vector2(this.Size.X / 2f, this.Size.Y / 2f);

            base.OnUpdateFrame(Args);
        }
    }
}
