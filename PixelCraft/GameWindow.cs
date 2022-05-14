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
            X = ((m.X - x) / (float)w - 0.5f) * 20 + vX;
            Y = ((y - m.Y) / (float)h + 0.535f) * 20 + vY;
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
        public Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
        Shader shader;
        private GameCursor GameCursor = new GameCursor();

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

        private float ViewX = 0;
        private float ViewY = 0;
        private float ViewZ = 25;

        private bool F3_Down = false;
        private double GameTime = 0;

        Stopwatch sw = new Stopwatch();
        Queue<int> avgFPS = new Queue<int>();


        public GameWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
            TextureBufferObject = GL.GenTexture();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState keybd = Keyboard.GetState();
            MouseState mouse = Mouse.GetCursorState();
            GameCursor.Update(mouse, X, Y, Width, Height, ViewX, ViewY);
            GameTime += e.Time;

            float coreX_prev = GameObjects["Player_Core"].Position.X;
            float coreY_prev = GameObjects["Player_Core"].Position.Y;

            foreach (var obj in GameObjects.Values)
            {
                obj.Update(GameObjects, keybd, GameCursor, GameTime);
            }

            float coreX = GameObjects["Player_Core"].Position.X;
            float coreY = GameObjects["Player_Core"].Position.Y;

            ShipCore core = (ShipCore)GameObjects["Player_Core"];
            Vector3 astPos = GameObjects["Asteroid"].Position;
            float soi = ((Asteroid)GameObjects["Asteroid"]).SOI;
            float dist = Vector3.Distance(new Vector3(coreX, coreY, astPos.Z), astPos);
            if (dist < soi + 3)
            {
                core.Vx += (coreX - astPos.X) / dist * core.Ax * 2f;
                core.Vy += (coreY - astPos.Y) / dist * core.Ay * 2f;
            }

            float porp = 0.05f;
            ViewX -= (((ViewX - coreX) * porp) + ((ViewX - coreX_prev) * porp)) / 2;
            ViewY -= (((ViewY - coreY) * porp) + ((ViewY - coreY_prev) * porp)) / 2;

            if (keybd.IsKeyDown(Key.F1) && core.Orbiters.Count > 0)
            {
                core.RemOrbiter(core.Orbiters[0]);
            }
            if (keybd.IsKeyDown(Key.F2))
            {
                ViewX = 0;
                ViewY = 0;
                //ViewZ = 15;
            }
            if (keybd.IsKeyDown(Key.F3) && !F3_Down)
            {
                Readout_Position.Enabled = !Readout_Position.Enabled;
                Readout_Rotation.Enabled = !Readout_Rotation.Enabled;
                Readout_FPS.Enabled = !Readout_FPS.Enabled;
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

            if (Readout_Position.Enabled)
            {
                Readout_Position.Text = "X= " + (int)ViewX + "  Y= " + (int)ViewY + "  Z= " + (int)ViewZ;
                Readout_Rotation.Text = "X= " + (int)(0 % 360) + "  Y= " + (int)(0 % 360);
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
            View_Translate = Matrix4.CreateTranslation(-ViewX, -ViewY, -ViewZ);
            View_Rotate = Matrix4.CreateRotationY(0 * 3.14f / 180) * Matrix4.CreateRotationX(0 * 3.14f / 180);
            Vector3 playerPos = new Vector3(ViewX, ViewY, ViewZ);

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
