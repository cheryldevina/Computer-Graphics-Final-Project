using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Render3D
{
    class Light
    {
        public static int Directional = 1;
        public static int Point = 2;
        public static int Spotlight = 3;

        public int LightType;
        public String Name;
        public String ParentName;

        // pos
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 InitialPosition;

        // color
        public Vector3 Ambient;
        public Vector3 Diffuse;
        public Vector3 Specular;

        // spotlight
        public float InnerCutOff;
        public float OuterCutOff;

        // attenuation
        public float Constant;
        public float Linear;
        public float Quadratic;
        public float FarPlane;

        public Matrix4 LightDirectionalMatrix;

        // animation
        public int TimelineHandle;
        public int ModelIndex;

        public Light(String Name)
        {
            this.Name = Name;
            ParentName = "";
            LightType = 0;

            Position = Vector3.Zero;
            InitialPosition = Vector3.Zero;
            Direction = new Vector3(0, -1, 0);

            Ambient = Vector3.One;
            Diffuse = Vector3.One;
            Specular = Vector3.One;

            InnerCutOff = 0.9f;
            OuterCutOff = 0.8f;

            Constant = 1.0f;
            Linear = 0.14f;
            Quadratic = 0.07f;
            FarPlane = 7500f;

            TimelineHandle = -1;

            LightDirectionalMatrix = Matrix4.Zero;
        }

        public static Light GenerateDirLight(
            String ParentName,
            String Name,
            int ModelIndex,
            Matrix4 Matrix,
            Vector3 Ambient,
            Vector3 Diffuse,
            Vector3 Specular,
            Vector3 Direction,
            Vector3 InitialPosition)
        {
            Light DirLight = new Light(Name);

            DirLight.Direction = Direction;
            DirLight.ParentName = ParentName;
            DirLight.ModelIndex = ModelIndex;
            DirLight.LightType = Light.Directional;
            DirLight.LightDirectionalMatrix = Matrix;
            DirLight.InitialPosition = InitialPosition;
            DirLight.Ambient = Ambient * Constants.LightScale;
            DirLight.Diffuse = Diffuse * Constants.LightScale;
            DirLight.Specular = Specular * Constants.LightScale;

            return DirLight;
        }

        public static Light GeneratePointLight(
            String ParentName,
            String Name,
            int ModelIndex,
            Matrix4 Matrix,
            Vector3 Ambient,
            Vector3 Diffuse,
            Vector3 Specular,
            Vector3 Position,
            Vector3 InitialPosition)
        {
            Light PointLight = new Light(Name);

            PointLight.Position = Position;
            PointLight.LightType = Light.Point;
            PointLight.ParentName = ParentName;
            PointLight.ModelIndex = ModelIndex;
            PointLight.LightDirectionalMatrix = Matrix;
            PointLight.InitialPosition = InitialPosition;
            PointLight.Ambient = Ambient * Constants.LightScale;
            PointLight.Diffuse = Diffuse * Constants.LightScale;
            PointLight.Specular = Specular * Constants.LightScale;

            return PointLight;
        }

        public static Light GenerateSpotlight(
            String ParentName,
            String Name,
            int ModelIndex,
            Matrix4 Matrix,
            Vector3 Ambient,
            Vector3 Diffuse,
            Vector3 Specular,
            Vector3 Position,
            Vector3 Direction,
            Vector3 InitialPosition,
            float InnerCutOff,
            float OuterCutOff)
        {
            Light Spotlight = new Light(Name);

            Spotlight.Position = Position;
            Spotlight.Direction = Direction;
            Spotlight.ModelIndex = ModelIndex;
            Spotlight.ParentName = ParentName;
            Spotlight.InnerCutOff = InnerCutOff;
            Spotlight.OuterCutOff = OuterCutOff;
            Spotlight.LightType = Light.Spotlight;
            Spotlight.LightDirectionalMatrix = Matrix;
            Spotlight.InitialPosition = InitialPosition;
            Spotlight.Ambient = Ambient * Constants.LightScale;
            Spotlight.Diffuse = Diffuse * Constants.LightScale;
            Spotlight.Specular = Specular * Constants.LightScale;

            return Spotlight;
        }

        public void RefreshLightMatrix(Node Parent)
        {
            LightDirectionalMatrix = Parent.TransformMatrix;

            if (Parent.Parent != null)
            {
                Node? Root = Parent.Parent;

                while (Root.Parent != null)
                {
                    LightDirectionalMatrix *= Root.TransformMatrix;
                    Root = Root.Parent;
                }

                Vector4 CalculatePos = new Vector4(InitialPosition, 1) * LightDirectionalMatrix;
                Vector4 CalculateDir = new Vector4(0, 1, 0, 1) * LightDirectionalMatrix;

                Position = CalculatePos.Xyz;
                Direction = Position - CalculateDir.Xyz;
            }
        }
    }
}
