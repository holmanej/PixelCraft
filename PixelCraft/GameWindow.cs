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
        public SpaceObject Cursor;

        public void Update(MouseState m, int x, int y, int w, int h, float vX, float vY, float vZ)
        {
            X = (((x - w / 2) / (float)w * 2) / vZ + vX);
            Y = ((-(y - h / 2) / (float)h * 2) / vZ + vY);
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
        public List<TextObject> UIElements = new List<TextObject>();
        Shader shader;
        private GameCursor GameCursor;

        public TextObject Readout_Position;
        public TextObject Readout_Gametime;
        public TextObject Readout_FPS;
        public TextObject Readout_SW;

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

        private float ViewX = 0f;
        private float ViewY = 0;
        private float ViewZ = 0.06f;

        private bool F3_Down = false;
        private double GameTime = 0;

        Stopwatch logic_sw = new Stopwatch();
        Stopwatch render_sw = new Stopwatch();
        int bulletCnt = 0;
        Queue<int> avgFPS = new Queue<int>();
        Queue<float> avgLGC = new Queue<float>();


        public GameWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            //MouseX = e.X;
            //MouseY = e.Y;
            MouseX += e.XDelta;
            MouseY += e.YDelta;
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ViewZ += ViewZ * 0.1f * Math.Sign(e.Delta);
            base.OnMouseWheel(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            logic_sw.Restart();
            float playerX = AllyAI.PlayerShip.Position.X;
            float playerY = AllyAI.PlayerShip.Position.Y;
            float porp = 0.05f;
            ViewX -= (ViewX - playerX) * porp;
            ViewY -= (ViewY - playerY) * porp;

            KeyboardState keybd = Keyboard.GetState();
            MouseState mouse = Mouse.GetCursorState();
            GameCursor.Update(mouse, MouseX, MouseY, Width, Height, ViewX, ViewY, ViewZ);
            //Debug.Print("Mx: {0} My: {1}", MouseX, MouseY);
            //Debug.WriteLine(GameCursor.X + " " + GameCursor.Y);
            GameTime += e.Time;
            //Debug.WriteLine(PlayerObject.Health);

            GameCursor.Cursor.SetPosition(GameCursor.X, GameCursor.Y, 0f);

            
            if (SpaceObjects.FindAll(o => o.ObjectState == SpaceObject.SpaceObjectState.DEAD).Count > 20)
            {
                SpaceObjects.Remove(SpaceObjects.Find(o => o.ObjectState == SpaceObject.SpaceObjectState.DEAD));
            }
            foreach (var obj in SpaceObjects)
            {
                obj.Update(SpaceObjects, keybd, GameCursor, GameTime);
            }
            EnemyAI.Update(SpaceObjects, AllyAI.PlayerShip);
            AllyAI.Update(SpaceObjects);
            

            if (keybd.IsKeyDown(Key.F1))
            {
            }
            if (keybd.IsKeyDown(Key.F2))
            {
                ViewX = 0;
                ViewY = 0;
                ViewZ = 0.1f;
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
            if (avgFPS.Count > 32)
            {
                avgFPS.Dequeue();
            }

            

            //if (Readout_Position.Visible)
            {
                Readout_Position.Text = "X=" + ViewX.ToString("F2") + "  Y=" + ViewY.ToString("F2") + "  Z=" + ViewZ.ToString("F3");
                Readout_Gametime.Text = "Gametime=" + GameTime.ToString("F1");
                Readout_FPS.Text = "FPS=" + (int)avgFPS.Average();
                Readout_SW.Text = "LGC=" + (avgLGC.Average() / 10000f / 60 * 100).ToString("F1") + "%  GUI=" + (render_sw.ElapsedTicks / 10000f).ToString("F2");
            }
            logic_sw.Stop();
            avgLGC.Enqueue(logic_sw.ElapsedTicks);
            if (avgLGC.Count > 32)
            {
                avgLGC.Dequeue();
            }
            base.OnUpdateFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.02f, 0.03f, 0.05f, 1.0f);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            CursorVisible = false;

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
            Readout_Position = new TextObject("POS XYZ", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.92f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) };
            Readout_Gametime = new TextObject("GAMETIME X.X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.84f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) };
            Readout_FPS = new TextObject("FPS X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.76f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) };
            Readout_SW = new TextObject("SW X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.68f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) };
            UIElements.Add(Readout_Position);
            UIElements.Add(Readout_Gametime);
            UIElements.Add(Readout_FPS);
            UIElements.Add(Readout_SW);

            // Objects
            GameCursor = new GameCursor();
            GameCursor.Cursor = new SpaceObject() { RenderSections = Program.Img2Sect(Program.Textures["Cursor"]), Shader = Program.Shaders["texture_shader"], Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.4f, 0.4f, 1f), Rotation = new Vector3(0, 0, 30f), SOI = 1f, Collidable = false };
            SpaceObjects.Add(GameCursor.Cursor);
            avgLGC.Enqueue(0);

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

        void RenderSpaceObjects(List<SpaceObject> objs, Vector3 playerPos)
        {
            foreach (var obj in objs)
            {
                if (obj.Visible && obj.Shader != null)
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
                        if (section.Visible)
                        {
                            shader.SetFloat("tex_alpha", section.Alpha);

                            if (section.ImageHandle == 0)
                            {
                                section.ImageHandle = GL.GenTexture();
                                GL.ActiveTexture(TextureUnit.Texture0);
                                GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, section.ImageSize.Width, section.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, section.ImageData);
                                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                            }
                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                            BufferObject(section.VBOData.ToArray());
                            GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength);
                        }
                    }
                }
            }
        }

        void RenderUIElements(List<TextObject> objs, Vector3 playerPos)
        {
            foreach (var obj in objs)
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
                        if (section.Visible)
                        {
                            shader.SetFloat("tex_alpha", section.Alpha);

                            if (section.ImageHandle == 0)
                            {
                                section.ImageHandle = GL.GenTexture();
                                GL.ActiveTexture(TextureUnit.Texture0);
                                GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, section.ImageSize.Width, section.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, section.ImageData);
                                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                            }
                            if (section.ImageUpdate)
                            {
                                GL.ActiveTexture(TextureUnit.Texture0);
                                GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, section.ImageSize.Width, section.ImageSize.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, section.ImageData);
                                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                            }

                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, section.ImageHandle);
                            BufferObject(section.VBOData.ToArray());
                            GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesLength);
                        }
                    }
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Model = Matrix4.CreateTranslation(0, 0, 0);
            View_Translate = Matrix4.CreateTranslation(-ViewX, -ViewY, 0);
            View_Scale = Matrix4.CreateScale(ViewZ, ViewZ, 1f);
            View_Rotate = Matrix4.CreateRotationY(0 * 3.14f / 180) * Matrix4.CreateRotationX(0 * 3.14f / 180);
            Vector3 playerPos = new Vector3(ViewX, ViewY, 0);

            GL.BindVertexArray(VertexArrayObject);
            RenderSpaceObjects(SpaceObjects, playerPos);
            RenderUIElements(UIElements, playerPos);
            bulletCnt = 0;
            render_sw.Restart();
            foreach (var list in SpaceObjects)
            {
                bulletCnt += list.Projectiles.Count;                
                RenderSpaceObjects(list.Projectiles, playerPos);
                RenderSpaceObjects(list.Modules, playerPos);
                render_sw.Start();
                RenderUIElements(list.UI, playerPos);
                render_sw.Stop();
            }
            render_sw.Stop();

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
