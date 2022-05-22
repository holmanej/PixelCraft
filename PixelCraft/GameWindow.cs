using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PixelCraft
{
    public class GameCursor
    {
        public float X;
        public float Y;
        public bool LeftPrev;
        public bool LeftPressed;
        public bool LeftReleased;
        public bool LeftDown;
        public bool RightPrev;
        public bool RightPressed;
        public bool RightReleased;
        public bool RightDown;
        public bool MiddlePressed;
        public bool MiddlePrev;
        public bool MiddleReleased;
        public bool MiddleDown;

        public void Update(MouseState m, int x, int y, int w, int h, float vX, float vY)
        {
            X = (((x - w / 2) / (float)w * 2) * 10 + vX);
            Y = ((-(y - h / 2) / (float)h * 2) * 10 + vY);
            LeftDown = m.LeftButton == ButtonState.Pressed;
            RightDown = m.RightButton == ButtonState.Pressed;
            MiddleDown = m.MiddleButton == ButtonState.Pressed;

            LeftPressed = !LeftPrev && LeftDown;
            LeftReleased = LeftPrev && !LeftDown;
            RightPressed = !RightPrev && RightDown;
            RightReleased = RightPrev && !RightDown;
            MiddlePressed = !MiddlePrev && MiddleDown;
            MiddleReleased = MiddlePrev && !MiddleDown;

            LeftPrev = LeftDown;
            RightPrev = RightDown;
            MiddlePrev = MiddleDown;
        }
    }

    class GameWindow : OpenTK.GameWindow
    {
        public List<SpaceObject> SpaceObjects = new List<SpaceObject>();
        public Dictionary<string, TextObject> UIElements = new Dictionary<string, TextObject>();
        public SpaceObject PlayerObject;
        Shader shader;
        private GameCursor GameCursor = new GameCursor();
        public SpaceObject CursorImage;

        public TextObject Readout_Position;
        public TextObject Readout_Gametime;
        public TextObject Readout_FPS;

        private const int VPosition_loc = 0;
        private const int VNormal_loc = 1;
        private const int VColor_loc = 2;
        private const int TexCoord_loc = 3;

        public Matrix4 Model;
        public Matrix4 View_Translate;
        public Matrix4 View_Scale;
        public Matrix4 View_Rotate;
        public Matrix4 Projection;

        private int VertexArrayObject;
        private int VertexBufferObject;
        private int VerticesLength;

        private int MouseX;
        private int MouseY;

        private float ViewX = -35f;
        private float ViewY = 0;
        private float ViewZ = 0f;

        private bool F3_Down = false;
        private double GameTime = 0;

        Stopwatch sw = new Stopwatch();
        Queue<int> avgFPS = new Queue<int>();


        public GameWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;
            base.OnMouseMove(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float playerX = PlayerObject.Position.X;
            float playerY = PlayerObject.Position.Y;
            float porp = 0.05f;
            ViewX -= (ViewX - playerX) * porp;
            ViewY -= (ViewY - playerY) * porp;

            KeyboardState keybd = Keyboard.GetState();
            MouseState mouse = Mouse.GetCursorState();
            GameCursor.Update(mouse, MouseX, MouseY, Width, Height, ViewX, ViewY);
            //Debug.Print("Mx: {0} My: {1}", MouseX, MouseY);
            //Debug.WriteLine(GameCursor.X + " " + GameCursor.Y);
            GameTime += e.Time;

            CursorImage.Position = new Vector3(GameCursor.X, GameCursor.Y, 0f);

            foreach (var obj in SpaceObjects)
            {
                obj.Update(SpaceObjects, keybd, GameCursor, GameTime);
            }

            if (keybd.IsKeyDown(Key.F1))
            {
            }
            if (keybd.IsKeyDown(Key.F2))
            {
                ViewX = 0;
                ViewY = 0;
                //ViewZ = 15;
            }
            if (keybd.IsKeyDown(Key.F3) && !F3_Down)
            {
                Readout_Position.Visible = !Readout_Position.Visible;
                Readout_Gametime.Visible = !Readout_Gametime.Visible;
                Readout_FPS.Visible = !Readout_FPS.Visible;
            }
            F3_Down = keybd.IsKeyDown(Key.F3);

            if (keybd.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            // Readouts
            avgFPS.Enqueue((int)RenderFrequency);
            if (avgFPS.Count > 256)
            {
                avgFPS.Dequeue();
            }

            if (Readout_Position.Visible)
            {
                Readout_Position.Text = "X= " + ViewX.ToString("F2") + "  Y= " + ViewY.ToString("F2") + "  Z= " + (int)ViewZ;
                //Readout_Rotation.Text = "X= " + (int)(0 % 360) + "  Y= " + (int)(0 % 360);
                Readout_Gametime.Text = "Gametime=" + GameTime.ToString("F1");
                Readout_FPS.Text = "FPS=" + (int)avgFPS.Average();
                //Debug.Print("FPS= {0}", (int)avgFPS.Average());
            }
            
            base.OnUpdateFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.02f, 0.03f, 0.05f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
            GL.Enable(EnableCap.AlphaTest);
            CursorGrabbed = false;
            CursorVisible = true;

            Projection = Matrix4.CreatePerspectiveFieldOfView(90f * 3.14f / 180f, Width / (float)Height, 0.01f, 10f);

            int stride = 12;
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.EnableVertexAttribArray(VPosition_loc);
            GL.VertexAttribPointer(VPosition_loc, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), 0);

            GL.EnableVertexAttribArray(VNormal_loc);
            GL.VertexAttribPointer(VNormal_loc, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(VColor_loc);
            GL.VertexAttribPointer(VColor_loc, 4, VertexAttribPointerType.Float, false, stride * sizeof(float), 6 * sizeof(float));

            GL.EnableVertexAttribArray(TexCoord_loc);
            GL.VertexAttribPointer(TexCoord_loc, 2, VertexAttribPointerType.Float, false, stride * sizeof(float), 10 * sizeof(float));
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Readouts
            Readout_Position = new TextObject("POS XYZ", Program.Fonts["times"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.92f, 0), Scale = new Vector3(0.8f, 0.07f, 1f), Color = Color.White, BGColor = Color.Black, Size = 24 };
            Readout_Gametime = new TextObject("GAMETIME X.X", Program.Fonts["times"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.84f, 0), Scale = new Vector3(0.8f, 0.07f, 1f), Color = Color.White, BGColor = Color.Black, Size = 24 };
            Readout_FPS = new TextObject("FPS X", Program.Fonts["times"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.76f, 0), Scale = new Vector3(0.8f, 0.07f, 1f), Color = Color.White, BGColor = Color.Black, Size = 24 };
            UIElements.Add("pos_rdout", Readout_Position);
            UIElements.Add("rot_rdout", Readout_Gametime);
            UIElements.Add("fps_rdout", Readout_FPS);

            // IMAGE GEN
            foreach (var obj in SpaceObjects)
            {
                foreach (var sect in obj.RenderSections)
                {
                    sect.ImageHandle = GL.GenTexture();
                    Debug.WriteLine("SO - " + sect.ImageHandle);
                    GL.BindTexture(TextureTarget.Texture2D, sect.ImageHandle);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, sect.ImageSize.Width, sect.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, sect.ImageData);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    sect.ImageUpdate = false;
                }

                foreach (TextObject uiText in obj.UI.Values)
                {
                    foreach (var sect in uiText.RenderSections)
                    {
                        sect.ImageHandle = GL.GenTexture();
                        Debug.WriteLine("UI - " + sect.ImageHandle);
                        GL.BindTexture(TextureTarget.Texture2D, sect.ImageHandle);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, sect.ImageSize.Width, sect.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, sect.ImageData);
                        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                        sect.ImageUpdate = false;
                    }
                }
            }

            foreach (var obj in UIElements.Values)
            {
                foreach (var sect in obj.RenderSections)
                {
                    sect.ImageHandle = GL.GenTexture();
                    Debug.WriteLine("DB - " + sect.ImageHandle);
                    GL.BindTexture(TextureTarget.Texture2D, sect.ImageHandle);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, sect.ImageSize.Width, sect.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, sect.ImageData);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    sect.ImageUpdate = false;
                }
            }

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteBuffer(VertexBufferObject);
            foreach (var obj in SpaceObjects)
            {
                obj.Shader.Dispose();
            }

            base.OnUnload(e);
        }

        public void BufferObject(float[] vertices)
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            VerticesLength = vertices.Length;
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Model = Matrix4.CreateTranslation(0, 0, 0);
            View_Translate = Matrix4.CreateTranslation(-ViewX, -ViewY, -ViewZ);
            View_Scale = Matrix4.CreateScale(0.1f, 0.1f, 1f);
            View_Rotate = Matrix4.CreateRotationY(0 * 3.14f / 180) * Matrix4.CreateRotationX(0 * 3.14f / 180);
            Vector3 playerPos = new Vector3(ViewX, ViewY, ViewZ);

            GL.BindVertexArray(VertexArrayObject);

            foreach (SpaceObject obj in SpaceObjects)
            {
                if (obj.Visible)
                {
                    shader = obj.Shader;
                    shader.Use();

                    shader.SetVector3("player_position", playerPos);

                    shader.SetMatrix4("view_translate", View_Translate);
                    shader.SetMatrix4("view_scale", View_Scale);
                    shader.SetMatrix4("view_rotate", View_Rotate);

                    shader.SetMatrix4("obj_translate", obj.matPos);
                    shader.SetMatrix4("obj_scale", obj.matScale);
                    shader.SetMatrix4("obj_rotate", obj.matRot);

                    foreach (RenderObject.Section section in obj.RenderSections)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                        BufferObject(section.VBOData.ToArray());
                        GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength);
                    }

                    foreach (TextObject uiText in obj.UI.Values)
                    {
                        shader = uiText.Shader;
                        shader.Use();

                        shader.SetMatrix4("obj_translate", uiText.matPos);
                        shader.SetMatrix4("obj_scale", uiText.matScale);

                        foreach (RenderObject.Section section in uiText.RenderSections)
                        {
                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                            if (section.ImageUpdate)
                            {
                                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, section.ImageSize.Width, section.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, section.ImageData);
                                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                                section.ImageUpdate = false;
                            }
                            BufferObject(section.VBOData.ToArray());
                            GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength);
                        }
                    }
                }
            }

            foreach (TextObject obj in UIElements.Values)
            {
                if (obj.Visible)
                {
                    shader = obj.Shader;
                    shader.Use();

                    shader.SetMatrix4("obj_translate", obj.matPos);
                    shader.SetMatrix4("obj_scale", obj.matScale);

                    foreach (RenderObject.Section section in obj.RenderSections)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                        if (section.ImageUpdate)
                        {
                            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, section.ImageSize.Width, section.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, section.ImageData);
                            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                            section.ImageUpdate = false;
                        }
                        BufferObject(section.VBOData.ToArray());
                        GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength);                        
                    }
                }
            }
            
            GL.BindVertexArray(0);

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }
    }
}
