using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Render3D
{
    class Texture
    {
        public int TextureHandle;

        public Texture(string Path, bool TextureYFlip = false)
        {
            Image<Rgba32> ImagePixels = Image.Load<Rgba32>(Path);
            if (TextureYFlip == true)
            {
                ImagePixels.Mutate(Image => Image.Flip(FlipMode.Vertical));
            }

            List<byte> ImageData = new List<byte>(4 * ImagePixels.Width * ImagePixels.Height);

            for (int Y = 0; Y < ImagePixels.Height; Y++)
            {
                for (int X = 0; X < ImagePixels.Width; X++)
                {
                    ImageData.Add(ImagePixels[X, Y].R);
                    ImageData.Add(ImagePixels[X, Y].G);
                    ImageData.Add(ImagePixels[X, Y].B);
                    ImageData.Add(ImagePixels[X, Y].A);
                }
            }

            int Width = ImagePixels.Width;
            int Height = ImagePixels.Height;

            byte[] Data = ImageData.ToArray();

            TextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            ImagePixels.Dispose();
        }

        public static List<Texture> GenerateMetalRoughSplit(string Path, bool TextureYFlip = false)
        {
            Image<Rgba32> ImagePixels = Image.Load<Rgba32>(Path);
            if (TextureYFlip == true)
            {
                ImagePixels.Mutate(Image => Image.Flip(FlipMode.Vertical));
            }

            List<byte> ImageDataMetal = new List<byte>(4 * ImagePixels.Width * ImagePixels.Height);
            List<byte> ImageDataRough = new List<byte>(4 * ImagePixels.Width * ImagePixels.Height);

            for (int y = 0; y < ImagePixels.Height; y++)
            {
                for (int x = 0; x < ImagePixels.Width; x++)
                {
                    ImageDataMetal.Add(ImagePixels[x, y].B);
                    ImageDataMetal.Add(ImagePixels[x, y].B);
                    ImageDataMetal.Add(ImagePixels[x, y].B);
                    ImageDataMetal.Add(ImagePixels[x, y].B);

                    ImageDataRough.Add(ImagePixels[x, y].G);
                    ImageDataRough.Add(ImagePixels[x, y].G);
                    ImageDataRough.Add(ImagePixels[x, y].G);
                    ImageDataRough.Add(ImagePixels[x, y].G);
                }
            }

            int Width = ImagePixels.Width;
            int Height = ImagePixels.Height;

            byte[] DataMetal = ImageDataMetal.ToArray();
            byte[] DataRough = ImageDataRough.ToArray();

            List<Texture> ReturnTexture = new List<Texture>();
            ReturnTexture.Add(new Texture(DataMetal, Width, Height));
            ReturnTexture.Add(new Texture(DataRough, Width, Height));
            ImagePixels.Dispose();

            return ReturnTexture;
        }

        public Texture(byte[] CompressedData, bool TextureYFlip = false)
        {
            Image<Rgba32> ImagePixels = Image.Load<Rgba32>(CompressedData);
            if (TextureYFlip == true) { 
                ImagePixels.Mutate(Image => Image.Flip(FlipMode.Vertical));
            }

            List<byte> ImageData = new List<byte>(4 * ImagePixels.Width * ImagePixels.Height);

            for (int Y = 0; Y < ImagePixels.Height; Y++)
            {
                for (int X = 0; X < ImagePixels.Width; X++)
                {
                    ImageData.Add(ImagePixels[X, Y].R);
                    ImageData.Add(ImagePixels[X, Y].G);
                    ImageData.Add(ImagePixels[X, Y].B);
                    ImageData.Add(ImagePixels[X, Y].A);
                }
            }

            int Width = ImagePixels.Width;
            int Height = ImagePixels.Height;
            byte[] Data = ImageData.ToArray();

            TextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            ImagePixels.Dispose();
        }

        public static List<Texture> GenerateMetalRoughSplit(byte[] CompressedData, bool TextureYFlip = false)
        {
            List<byte> ImageDataMetal;
            List<byte> ImageDataRough;

            byte[] DataMetal;
            byte[] DataRough;
            int Width;
            int Height;

            Image<Rgba32> ImagePixels = Image.Load<Rgba32>(CompressedData);
            if (TextureYFlip == true)
            {
                ImagePixels.Mutate(Image => Image.Flip(FlipMode.Vertical));
            }

            ImageDataMetal = new List<byte>(4 * ImagePixels.Width * ImagePixels.Height);
            ImageDataRough = new List<byte>(4 * ImagePixels.Width * ImagePixels.Height);

            for (int Y = 0; Y < ImagePixels.Height; Y++)
            {
                for (int X = 0; X < ImagePixels.Width; X++)
                {
                    ImageDataMetal.Add(ImagePixels[X, Y].B);
                    ImageDataMetal.Add(ImagePixels[X, Y].B);
                    ImageDataMetal.Add(ImagePixels[X, Y].B);
                    ImageDataMetal.Add(ImagePixels[X, Y].B);

                    ImageDataRough.Add(ImagePixels[X, Y].G);
                    ImageDataRough.Add(ImagePixels[X, Y].G);
                    ImageDataRough.Add(ImagePixels[X, Y].G);
                    ImageDataRough.Add(ImagePixels[X, Y].G);
                }
            }

            Width = ImagePixels.Width;
            Height = ImagePixels.Height;

            DataMetal = ImageDataMetal.ToArray();
            DataRough = ImageDataRough.ToArray();

            List<Texture> ReturnTexture = new List<Texture>();
            ReturnTexture.Add(new Texture(DataMetal, Width, Height));
            ReturnTexture.Add(new Texture(DataRough, Width, Height));
            ImagePixels.Dispose();

            return ReturnTexture;
        }

        public Texture(byte[] Data, int Width, int Height)
        {
            TextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
    }
}
