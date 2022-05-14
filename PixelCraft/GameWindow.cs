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
    class GameWindow : OpenTK.GameWindow
    {
        public Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
        Shader shader;

        public TextObject Readout_Position;
        public TextObject Readout_Rotation;
        public TextObject Readout_FPS;

        private const int VPosition_loc = 0;
        private const int VNormal_loc = 1;
        private const int VColor_loc = 2;
        private const int TexCoord_loc = 3;

        public Matrix4 Model;
        public Matrix4 View_Translate;
        public Matrix4 View_Rotate;
        public Matrix4 Projection;

        private int VertexArrayObject;
        private int VertexBufferObject;
        private int TextureBufferObject;
        private int VerticesLength;

        private float YRotation = 0;
        private float XRotation = 0;
        private float XPosition = -2;
        private float YPosition = -24;
        private float ZPosition = 15;
        private float XCenter = 0;
        private float YCenter = 0;
        private float CursorX = 0;
        private float CursorY = 0;
        private float XSensitivity = 0.3f;
        private float YSensitivity = 0.25f;
        private float WSensitivity = 0.1f;

        private bool F3_Down = false;
        private bool F4_Down = false;
        private bool CursorLocked = true;
        private double GameTime = 0;

        Stopwatch sw = new Stopwatch();
        Queue<int> avgFPS = new Queue<int>();


        public GameWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
            TextureBufferObject = GL.GenTexture();

            MouseMove += WaveSimWindow_MouseMove;
        }

        private void WaveSimWindow_MouseMove(object sender, MouseMoveEventArgs e)
        {
            CursorX = e.X - XCenter;
            CursorY = e.Y - YCenter;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();
            GameTime += e.Time;

            //if (CursorLocked)
            //{
            //    YRotation += CursorX * XSensitivity;
            //    XRotation += CursorY * YSensitivity;
            //    Mouse.SetPosition(Width / 2 + X, Height / 2 + Y);
            //    XCenter = Width / 2 - 8;
            //    YCenter = Height / 2 - 31;
            //}

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            float dsin = (float)Math.Sin(YRotation * 3.14f / 180) * WSensitivity;
            float dcos = (float)Math.Cos(YRotation * 3.14f / 180) * WSensitivity;
            float xMove = 0;
            float yMove = 0;
            float zMove = 0;

            if (input.IsKeyDown(Key.W))
            {
                //yMove += WSensitivity;
                yMove = 1;
            }
            if (input.IsKeyDown(Key.S))
            {
                //yMove -= WSensitivity;
                yMove = -1;
            }
            if (input.IsKeyDown(Key.A))
            {
                //xMove -= WSensitivity;
                xMove = -1;
            }
            if (input.IsKeyDown(Key.D))
            {
                //xMove += WSensitivity;
                xMove = 1;
            }

            float coreX_prev = GameObjects["Player_Core"].Position.X;
            float coreY_prev = GameObjects["Player_Core"].Position.Y;

            ((SpaceObject)GameObjects["Player_Core"]).Thrust((int)xMove, (int)yMove);

            float coreX = GameObjects["Player_Core"].Position.X;
            float coreY = GameObjects["Player_Core"].Position.Y;

            XPosition -= (((XPosition - coreX) * 0.05f) + ((XPosition - coreX_prev) * 0.05f)) / 2;
            YPosition -= (((YPosition - coreY) * 0.05f) + ((YPosition - coreY_prev) * 0.05f)) / 2;
            ZPosition += zMove;

            float sin = (float)Math.Sin(GameTime / 4 + Math.PI * 2 / 3);
            float cos = (float)Math.Cos(GameTime / 4 + Math.PI * 2 / 3);
            GameObjects["Player_Turret"].Position = new Vector3(coreX + sin, coreY + cos, -10);
            sin = (float)Math.Sin(GameTime / 4 + Math.PI * 4 / 3);
            cos = (float)Math.Cos(GameTime / 4 + Math.PI * 4 / 3);
            GameObjects["Player_Fab"].Position = new Vector3(coreX + sin, coreY + cos, -10);
            sin = (float)Math.Sin(GameTime / 4 + Math.PI * 6 / 3);
            cos = (float)Math.Cos(GameTime / 4 + Math.PI * 6 / 3);
            GameObjects["Player_Exca"].Position = new Vector3(coreX + sin, coreY + cos, -10);
            
            GameObjects["Player_Core"].Rotate(0f, 0f, (float)(1.1 * (Math.Sin(GameTime) + Math.Cos(GameTime*2.5))));
            GameObjects["Asteroid"].Rotate(0f, 0f, -0.002f);

            if (input.IsKeyDown(Key.F1))
            {
                XRotation = 0;
                YRotation = 0;
            }
            if (input.IsKeyDown(Key.F2))
            {
                XPosition = -2;
                YPosition = -24;
                ZPosition = 15;
            }
            if (input.IsKeyDown(Key.F3) && !F3_Down)
            {
                Readout_Position.Enabled = !Readout_Position.Enabled;
                Readout_Rotation.Enabled = !Readout_Rotation.Enabled;
                Readout_FPS.Enabled = !Readout_FPS.Enabled;
            }
            F3_Down = input.IsKeyDown(Key.F3);

            //if (input.IsKeyDown(Key.F4) && !F4_Down)
            //{
            //    CursorGrabbed = !CursorGrabbed;
            //    CursorVisible = !CursorVisible;
            //    CursorLocked = !CursorLocked;
            //}
            //F4_Down = input.IsKeyDown(Key.F4);

            //if (input.IsKeyDown(Key.F5))
            //{
            //    WSensitivity -= 0.1f;
            //}
            //if (input.IsKeyDown(Key.F6))
            //{
            //    WSensitivity += 0.1f;
            //}

            // Readouts
            avgFPS.Enqueue((int)RenderFrequency);
            if (avgFPS.Count > 256)
            {
                avgFPS.Dequeue();
            }

            if (Readout_Position.Enabled)
            {
                Readout_Position.Text = "X= " + (int)XPosition + "  Y= " + (int)YPosition + "  Z= " + (int)ZPosition;
                Readout_Rotation.Text = "X= " + (int)(XRotation % 360) + "  Y= " + (int)(YRotation % 360);
                Readout_FPS.Text = "FPS= " + (int)avgFPS.Average();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.02f, 0.01f, 0.03f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            CursorGrabbed = false;
            CursorVisible = true;

            Projection = Matrix4.CreatePerspectiveFieldOfView(45f * 3.14f / 180f, Width / (float)Height, 0.01f, 100f);

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
            Readout_Position = new TextObject("Hello, World!", Program.Fonts["times"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-1, 0.95f, 0), Color = Color.White, BGColor = Color.Black, Size = 8 };
            Readout_Rotation = new TextObject("*", Program.Fonts["times"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-1, 0.9f, 0), Color = Color.White, BGColor = Color.Black, Size = 8 };
            Readout_FPS = new TextObject("*", Program.Fonts["times"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-1, 0.85f, 0), Color = Color.White, BGColor = Color.Black, Size = 8 };
            GameObjects.Add("pos_rdout", Readout_Position);
            GameObjects.Add("rot_rdout", Readout_Rotation);
            GameObjects.Add("fps_rdout", Readout_FPS);

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteTexture(TextureBufferObject);
            foreach (var obj in GameObjects.Values)
            {
                obj.Shader.Dispose();
            }

            base.OnUnload(e);
        }

        public void BufferObject(float[] vertices, byte[] pixels, Size texSize)
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            VerticesLength = vertices.Length;
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2D, TextureBufferObject);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texSize.Width, texSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Model = Matrix4.CreateTranslation(0, 0, 0);
            View_Translate = Matrix4.CreateTranslation(-XPosition, -YPosition, -ZPosition);
            View_Rotate = Matrix4.CreateRotationY(YRotation * 3.14f / 180) * Matrix4.CreateRotationX(XRotation * 3.14f / 180);
            Vector3 playerPos = new Vector3(XPosition, YPosition, ZPosition);

            GL.BindVertexArray(VertexArrayObject);


            foreach (GameObject gameObject in GameObjects.Values)
            {
                if (gameObject.Enabled)
                {

                    shader = gameObject.Shader;
                    shader.Use();

                    shader.SetVector3("player_position", playerPos);

                    shader.SetMatrix4("model", Model);
                    shader.SetMatrix4("view_translate", View_Translate);
                    shader.SetMatrix4("view_rotate", View_Rotate);
                    shader.SetMatrix4("projection", Projection);

                    shader.SetMatrix4("obj_translate", gameObject.matPos);
                    shader.SetMatrix4("obj_scale", gameObject.matScale);
                    shader.SetMatrix4("obj_rotate", gameObject.matRot);

                    shader.SetTexture("texture0", 0);

                    foreach (GameObject.Section section in gameObject.RenderSections)
                    {
                        BufferObject(section.VBOData.ToArray(), section.ImageData, section.ImageSize);
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
