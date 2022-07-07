using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Render3D
{
    class Material
    {
        // standard components
        public Vector3 Ambient;

        public Vector3 Diffuse;
        public string DiffuseMap;
        public int UseDiffuseMap;

        public Vector3 Specular;
        public string SpecularMap;
        public float Shininess;

        public Vector3 Emissive;
        public string EmissiveMap;

        public string NormalMap;
        public int UseNormalMap;

        public string OpacityMap;
        public float Alpha;

        // pbr components
        public int IsPBR;

        public float Metalness;
        public string MetalnessMap;

        public float Roughness;
        public string RoughnessMap;

        // others
        public string DisplacementMap;
        public float DisplacementScale;

        public Material(Assimp.Material Material)
        {
            Ambient = Vector3.One * Constants.AmbientValue;
            Diffuse = Vector3.One;
            Specular = Vector3.One;
            Emissive = Vector3.Zero;
            Metalness = 0.5f;
            Roughness = 0.5f;
            Alpha = 1;
            DisplacementScale = 0.1f;
            Shininess = 32;
            UseDiffuseMap = 0;
            UseNormalMap = 0;
            IsPBR = 0;

            if (Material.HasColorAmbient)
            {
                Ambient.X = Material.ColorAmbient.R;
                Ambient.Y = Material.ColorAmbient.G;
                Ambient.Z = Material.ColorAmbient.B;
            }

            if (Material.HasColorDiffuse)
            {
                Diffuse.X = Material.ColorDiffuse.R;
                Diffuse.Y = Material.ColorDiffuse.G;
                Diffuse.Z = Material.ColorDiffuse.B;
                Alpha = Material.ColorDiffuse.A;
            }

            if (Material.HasColorSpecular)
            {
                Specular.X = Material.ColorSpecular.R;
                Specular.Y = Material.ColorSpecular.G;
                Specular.Z = Material.ColorSpecular.B;
            }

            if (Material.HasColorEmissive)
            {
                Specular.X = Material.ColorEmissive.R;
                Specular.Y = Material.ColorEmissive.G;
                Specular.Z = Material.ColorEmissive.B;
            }

            if (Material.HasShininess)
            {
                Shininess = Material.Shininess;
            }

            if (Material.HasOpacity)
            {
                Alpha = Material.Opacity;
            }

            if (Material.HasBumpScaling)
            {
                DisplacementScale = Material.BumpScaling;
            }

            if (Material.HasProperty("$mat.gltf.pbrMetallicRoughness.baseColorFactor,0,0"))
            {
                IsPBR = 1;
                Assimp.Color4D Color = Material.GetProperty("$mat.gltf.pbrMetallicRoughness.baseColorFactor,0,0").GetColor4DValue();
                Diffuse.X = Color.R;
                Diffuse.Y = Color.G;
                Diffuse.Z = Color.B;
                Alpha = Color.A;
            }

            if (Material.HasProperty("$mat.gltf.pbrMetallicRoughness.metallicFactor,0,0"))
            {
                IsPBR = 1;
                Metalness = Material.GetProperty("$mat.gltf.pbrMetallicRoughness.metallicFactor,0,0").GetFloatValue();
            }

            if (Material.HasProperty("$mat.gltf.pbrMetallicRoughness.roughnessFactor,0,0"))
            {
                IsPBR = 1;
                Roughness = Material.GetProperty("$mat.gltf.pbrMetallicRoughness.roughnessFactor,0,0").GetFloatValue();
            }

            // reset parameters related if uses map except displacement

            DiffuseMap = "WHITE_DEFAULT_MAP";
            SpecularMap = "WHITE_DEFAULT_MAP";
            EmissiveMap = "BLACK_DEFAULT_MAP";
            NormalMap = "WHITE_DEFAULT_MAP";
            OpacityMap = "WHITE_DEFAULT_MAP";
            MetalnessMap = "WHITE_DEFAULT_MAP";
            RoughnessMap = "WHITE_DEFAULT_MAP";
            DisplacementMap = "BLACK_DEFAULT_MAP";

            if (Material.HasTextureDiffuse)
            {
                UseDiffuseMap = 1;
                DiffuseMap = Material.TextureDiffuse.FilePath;
                Diffuse = Vector3.One;
            }

            if (Material.HasTextureNormal)
            {
                UseNormalMap = 1;
                NormalMap = Material.TextureNormal.FilePath;
            }

            if (Material.HasTextureSpecular)
            {
                SpecularMap = Material.TextureSpecular.FilePath;
                Specular = Vector3.One;
            }

            if (Material.HasTextureEmissive)
            {
                EmissiveMap = Material.TextureEmissive.FilePath;
                Emissive = Vector3.One;
            }

            if (Material.HasTextureDisplacement)
            {
                DisplacementMap = Material.TextureDisplacement.FilePath;
            }

            if (Material.HasTextureOpacity)
            {
                OpacityMap = Material.TextureOpacity.FilePath;
                Alpha = 1;
            }

            if (Material.GetMaterialTexture(Assimp.TextureType.Metalness, 0, out Assimp.TextureSlot MetalnessSlot))
            {
                IsPBR = 1;
                MetalnessMap = MetalnessSlot.FilePath;
                Metalness = 1;
            };


            if (Material.GetMaterialTexture(Assimp.TextureType.Roughness, 0, out Assimp.TextureSlot RoughnessSlot))
            {
                IsPBR = 1;
                RoughnessMap = RoughnessSlot.FilePath;
                Roughness = 1;
            };

            if (Material.GetMaterialTexture(Assimp.TextureType.Unknown, 0, out Assimp.TextureSlot MetalnessRoughnessSlot))
            {
                IsPBR = 1;
                MetalnessMap = MetalnessRoughnessSlot.FilePath + "_Metalness";
                RoughnessMap = MetalnessRoughnessSlot.FilePath + "_Roughness";
                Metalness = 1;
                Roughness = 1;
            };

            if (Material.GetMaterialTexture(Assimp.TextureType.BaseColor, 0, out Assimp.TextureSlot BaseColorSlot))
            {
                IsPBR = 1;
                DiffuseMap = BaseColorSlot.FilePath;
                Diffuse = Vector3.One;
            };
        }

    }

}
