using OpenTK;
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
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace PixelCraft
{
    public class RenderObject
    {
        public class Section
        {
            public List<float> VBOData;
            public List<float> NormalData;
            public byte[] ImageData;
            public Size ImageSize;
            public int ImageHandle;
            public bool ImageUpdate;
            public float Metal;
            public float Rough;
            public float Alpha = 1f;
            public bool Visible = true;

            public Section() { }

            public Section(Section section, bool visible)
            {
                VBOData = section.VBOData;
                NormalData = section.NormalData;
                ImageData = section.ImageData;
                ImageSize = section.ImageSize;
                ImageHandle = 0;
                ImageUpdate = true;
                Metal = section.Metal;
                Rough = section.Rough;
                Alpha = 1f;
                Visible = visible;
            }

            public Section(Image img)
            {
                Bitmap bmp;
                using (MemoryStream stream = new MemoryStream())
                {
                    img.Save(stream, ImageFormat.Bmp);
                    bmp = (Bitmap)Image.FromStream(stream);
                    //img.Save("stream.bmp", ImageFormat.Bmp);
                }

                bmp.MakeTransparent(bmp.GetPixel(0, 0));
                BitmapData fileData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
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
                Metal = 0.5f;
                Rough = 0.5f;

                bmp.Dispose();
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

        public bool Enabled = true;

        public float Distance(Vector3 target)
        {
            return Vector3.Distance(Position, target);
        }

        public float dTheta(Vector3 target)
        {
            float dx = target.X - Position.X;
            float dy = target.Y - Position.Y;
            float theta = (float)Math.Atan(dy / dx);
            if (dx < 0) { theta += 3.14f; }
            float dt = theta * 180f / 3.14f - Rotation.Z - 90;
            if (Math.Abs(dt) > 180) { dt -= Math.Sign(dt) * 360; }
            return dt;
        }

        public static float Mag(float a, float b)
        {
            return (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        public void SetAlpha(float alpha)
        {
            foreach (var sect in RenderSections) { sect.Alpha = alpha; }
        }

        public void AdjustAlpha(float dA)
        {
            foreach (var sect in RenderSections) { sect.Alpha += dA; }
        }

        public void DeleteTex()
        {
            foreach (var sect in RenderSections)
            {
                GL.DeleteTexture(sect.ImageHandle);
            }
        }

        public void Render(int vArrayObj)
        {
            Stopwatch sw = new Stopwatch();
            if (Enabled && Shader != null)
            {
                Shader.Use();
                Shader.SetMatrix4("obj_translate", matPos);
                Shader.SetMatrix4("obj_scale", matScale);
                Shader.SetMatrix4("obj_rotate", matRot);
                foreach (Section section in RenderSections)
                {
                    if (section.Visible)
                    {
                        Shader.SetFloat("tex_alpha", section.Alpha);

                        if (section.ImageHandle == 0)
                        {
                            section.ImageHandle = GL.GenTexture();
                            section.ImageUpdate = true;
                        }

                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);

                        if (section.ImageUpdate)
                        {
                            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, section.ImageSize.Width, section.ImageSize.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, section.ImageData);
                            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                            section.ImageUpdate = false;
                        }
                        sw.Start();
                        
                        GL.BindVertexArray(vArrayObj);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vArrayObj);
                        GL.BufferData(BufferTarget.ArrayBuffer, section.VBOData.Count * sizeof(float), section.VBOData.ToArray(), BufferUsageHint.DynamicDraw);
                        GL.DrawArrays(PrimitiveType.Triangles, 0, section.VBOData.Count);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                        sw.Stop();
                    }
                }
            }
            
            //Debug.WriteLine(sw.ElapsedTicks);
        }

        public void Render(Section section, int vArrayObj)
        {
            Stopwatch sw = new Stopwatch();
            if (Enabled && Shader != null && section != null)
            {
                Shader.Use();
                Shader.SetMatrix4("obj_translate", matPos);
                Shader.SetMatrix4("obj_scale", matScale);
                Shader.SetMatrix4("obj_rotate", matRot);

                if (section.Visible)
                {
                    Shader.SetFloat("tex_alpha", section.Alpha);

                    if (section.ImageHandle == 0)
                    {
                        section.ImageHandle = GL.GenTexture();
                        section.ImageUpdate = true;
                    }

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);

                    if (section.ImageUpdate)
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, section.ImageSize.Width, section.ImageSize.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, section.ImageData);
                        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                        section.ImageUpdate = false;
                    }
                    sw.Start();

                    GL.BindVertexArray(vArrayObj);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vArrayObj);
                    GL.BufferData(BufferTarget.ArrayBuffer, section.VBOData.Count * sizeof(float), section.VBOData.ToArray(), BufferUsageHint.DynamicDraw);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, section.VBOData.Count);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    sw.Stop();
                }
            }

            //Debug.WriteLine(sw.ElapsedTicks);
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
            _Position.X = Position.X + x;
            _Position.Y = Position.Y + y;
            _Position.Z = Position.Z + z;
            matPos = Matrix4.CreateTranslation(_Position);
        }

        public void ReSize(float x, float y, float z)
        {
            _Scale.X = Scale.X + x;
            _Scale.Y = Scale.Y + y;
            _Scale.Z = Scale.Z + z;
            matScale = Matrix4.CreateScale(_Scale);
        }

        public void Rotate(float x, float y, float z)
        {
            _Rotation.X = Rotation.X + x;
            _Rotation.Y = Rotation.Y + y;
            _Rotation.Z = Rotation.Z + z;
            matRot = Matrix4.CreateRotationX(_Rotation.X * 3.14f / 180) * Matrix4.CreateRotationY(_Rotation.Y * 3.14f / 180) * Matrix4.CreateRotationZ(_Rotation.Z * 3.14f / 180);
        }

        public void SetPosition(float x, float y, float z)
        {
            _Position.X = x;
            _Position.Y = y;
            _Position.Z = z;
            matPos = Matrix4.CreateTranslation(_Position);
        }

        public void SetScale(float x, float y, float z)
        {
            _Scale.X = x;
            _Scale.Y = y;
            _Scale.Z = z;
            matScale = Matrix4.CreateScale(_Scale);
        }

        public void SetRotation(float x, float y, float z)
        {
            _Rotation.X = x;
            _Rotation.Y = y;
            _Rotation.Z = z;
            matRot = Matrix4.CreateRotationX(_Rotation.X * 3.14f / 180) * Matrix4.CreateRotationY(_Rotation.Y * 3.14f / 180) * Matrix4.CreateRotationZ(_Rotation.Z * 3.14f / 180);
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
