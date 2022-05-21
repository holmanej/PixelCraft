﻿using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    public abstract class RenderObject
    {
        public class Section
        {
            public List<float> VBOData;
            public List<float> NormalData;
            public byte[] ImageData;
            public Size ImageSize;
            public int ImageHandle;
            public bool ImageUpdate;
            public float metal;
            public float rough;

            public Section() { }

            public Section(Image img)
            {
                Bitmap bmp;
                using (MemoryStream stream = new MemoryStream())
                {
                    img.Save(stream, ImageFormat.Bmp);
                    bmp = (Bitmap)Image.FromStream(stream);
                    img.Save("stream.bmp", ImageFormat.Bmp);
                }

                bmp.MakeTransparent(bmp.GetPixel(0, 0));
                BitmapData fileData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                byte[] imgData = new byte[bmp.Width * bmp.Height * 4];
                Marshal.Copy(fileData.Scan0, imgData, 0, imgData.Length);
                bmp.UnlockBits(fileData);

                float w = 1f;// bmp.Width;
                float h = 1f;// bmp.Height;
                VBOData = new List<float>()
                {
                    -w, -h, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                    w, -h, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                    -w, h, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    w, -h, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                    -w, h, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    w, h, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0
                };

                ImageData = imgData;
                ImageSize = bmp.Size;
                metal = 0.5f;
                rough = 0.5f;

                //Debug.WriteLine("Dim: {2}, {3} Size: {0} Data: {1}", ImageSize, ImageData.Length, w, h);
            }
        }

        public List<Section> RenderSections;
        public Shader Shader;

        public Vector3 _Position = new Vector3(0, 0, 0);
        public Vector3 _Scale = new Vector3(1, 1, 1);
        public Vector3 _Rotation = new Vector3(0, 0, 0);

        public Matrix4 matPos = Matrix4.Identity;
        public Matrix4 matScale = Matrix4.Identity;
        public Matrix4 matRot = Matrix4.Identity;
        public Matrix4 colRot = Matrix4.Identity;

        public bool Visible = true;

        public abstract void Update(List<SpaceObject> objs, KeyboardState keybd, GameCursor cursor, double gametime);

        public float Distance(Vector3 target)
        {
            return Vector3.Distance(Position, target);
        }

        public static float Mag(float a, float b)
        {
            return (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        private void CalculateNormalData()
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Restart();
            foreach (Section section in RenderSections)
            {
                section.NormalData = new List<float>();

                for (int i = 0; i < section.VBOData.Count; i += 36)
                {
                    Vector3 a = new Vector3(section.VBOData[i], section.VBOData[i + 1], section.VBOData[i + 2]);
                    Vector3 b = new Vector3(section.VBOData[i + 12], section.VBOData[i + 13], section.VBOData[i + 14]);
                    Vector3 c = new Vector3(section.VBOData[i + 24], section.VBOData[i + 25], section.VBOData[i + 26]);
                    Vector3 n = Vector3.Cross(b - a, c - a);

                    a = (new Vector4(a.X * _Scale.X, a.Y * _Scale.Y, a.Z * _Scale.Z, 1) * matRot).Xyz;
                    n = (new Vector4(n, 1) * matRot).Xyz;

                    section.NormalData.Add(a.X + _Position.X);
                    section.NormalData.Add(a.Y + _Position.Y);
                    section.NormalData.Add(a.Z + _Position.Z);
                    section.NormalData.Add((float)Math.Round(n.X, 2));
                    section.NormalData.Add((float)Math.Round(n.Y, 2));
                    section.NormalData.Add((float)Math.Round(n.Z, 2));
                    section.NormalData.Add(n.Length);
                }
            }
            //sw.Stop();
            //Debug.WriteLine("recalc normals: " + sw.Elapsed);
        }

        public void Translate(float x, float y, float z)
        {
            Vector3 newPos = new Vector3(Position.X + x, Position.Y + y, Position.Z + z);
            Position = newPos;
        }

        public void ReSize(float x, float y, float z)
        {
            Scale = new Vector3(Scale.X + x, Scale.Y + y, Scale.Z + z);
        }

        public void Rotate(float x, float y, float z)
        {
            Rotation = new Vector3(Rotation.X + x, Rotation.Y + y, Rotation.Z + z);
        }

        public Vector3 Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                    matPos = Matrix4.CreateTranslation(_Position);
                    //CalculateNormalData();
                }
            }
        }

        public Vector3 Scale
        {
            get { return _Scale; }
            set
            {
                if (value != _Scale)
                {
                    _Scale = value;
                    matScale = Matrix4.CreateScale(_Scale);
                    //CalculateNormalData();
                }
            }
        }

        public Vector3 Rotation
        {
            get { return _Rotation; }
            set
            {
                if (value != _Rotation)
                {
                    _Rotation = value;
                    matRot = Matrix4.CreateRotationX(_Rotation.X * 3.14f / 180) * Matrix4.CreateRotationY(_Rotation.Y * 3.14f / 180) * Matrix4.CreateRotationZ(_Rotation.Z * 3.14f / 180);
                    //CalculateNormalData();
                }
            }
        }
    }
}
