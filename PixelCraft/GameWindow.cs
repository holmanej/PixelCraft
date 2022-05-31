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
        public float Mx;
        public float My;
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
        public RenderObject Cursor;

        public void Update(MouseState m, int x, int y, int w, int h, float vX, float vY, float vZ)
        {
            Mx = (x - w / 2) / (float)w * 2;
            My = -(y - h / 2) / (float)h * 2;
            X = (Mx + 0.2f) / vZ + vX;
            Y = My / vZ + vY;
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

    public class GameWindow : OpenTK.GameWindow
    {
        public List<SpaceObject> SpaceObjects = new List<SpaceObject>();
        public GameCursor GameCursor;

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

        private int MouseX;
        private int MouseY;
        public bool DebugShown = false;

        public float ViewX = 0f;
        public float ViewY = 0;
        public float ViewZ = 0.06f;

        public double GameTime = 0;

        public Stopwatch logic_sw = new Stopwatch();
        public Stopwatch render_sw = new Stopwatch();
        public Queue<int> avgFPS = new Queue<int>();
        public Queue<float> avgLGC = new Queue<float>();
        public Queue<float> avgRNDR = new Queue<float>();
        public int bulletCnt = 0;


        public GameWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            MouseX += e.XDelta;
            MouseY += e.YDelta;
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (ViewZ >= 0.01f && ViewZ <= 0.4f)
            {
                ViewZ += ViewZ * 0.1f * Math.Sign(e.Delta);
            }
            base.OnMouseWheel(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                WorldManager.ChangeLevel("Title");
            }

            if (e.Key == Key.F2)
            {
                WorldManager.RestartLevel();
            }

            if (e.Key == Key.F3)
            {
                DebugShown = !DebugShown;
                UIManager.UIGroups["DebugPanel"].Enabled = DebugShown;
            }

            if (e.Key == Key.Escape)
            {
                Exit();
            }
            base.OnKeyUp(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            logic_sw.Restart();

            // Accumulate gametime
            GameTime += e.Time;

            // Update view coordinates
            if (ViewZ < 0.01f) { ViewZ += 0.0001f; }
            if (ViewZ > 0.4f) { ViewZ -= 0.01f; }
            ViewX -= (ViewX - AllyAI.PlayerShip.Position.X) * 0.05f;
            ViewY -= (ViewY - AllyAI.PlayerShip.Position.Y) * 0.05f;

            // Update inputs
            KeyboardState keybd = Keyboard.GetState();
            GameCursor.Update(Mouse.GetCursorState(), MouseX, MouseY, Width, Height, ViewX, ViewY, ViewZ);
            GameCursor.Cursor.SetPosition(GameCursor.Mx, GameCursor.My, 0f);

            // Run object updates
            WorldManager.LevelAI();
            for (int i = 0; i < SpaceObjects.Count; i++)
            {
                SpaceObjects[i].Update(SpaceObjects, keybd, GameCursor, GameTime);
            }
            EnemyAI.Update();
            AllyAI.Update();
            foreach (var ui in UIManager.UIGroups.Values)
            {
                if (ui.Enabled) { ui.UpdateDel(); }
            }

            // Debug stats
            avgFPS.Enqueue((int)RenderFrequency);
            if (avgFPS.Count > 32)
            {
                avgFPS.Dequeue();
            }

            logic_sw.Stop();
            avgLGC.Enqueue(logic_sw.ElapsedTicks);
            if (avgLGC.Count > 32)
            {
                avgLGC.Dequeue();
            }
            avgRNDR.Enqueue(render_sw.ElapsedTicks);
            if (avgRNDR.Count > 32)
            {
                avgRNDR.Dequeue();
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

            // Objects
            GameCursor = new GameCursor();
            GameCursor.Cursor = new SpaceObject() { RenderSections = Program.Img2Sect(Program.Textures["Cursor"]), Shader = Program.Shaders["debugText_shader"], Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.02f, 0.02f, 0f), Rotation = new Vector3(0, 0, 30f), SOI = 1f, Collidable = false };
            UIManager.Cursor = GameCursor;
            UIManager.Gwin = this;

            avgFPS.Enqueue(0);
            avgLGC.Enqueue(0);
            avgRNDR.Enqueue(0);

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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Model = Matrix4.CreateTranslation(0, 0, 0);
            View_Translate = Matrix4.CreateTranslation(-ViewX - 0.2f / ViewZ, -ViewY, 0);
            View_Scale = Matrix4.CreateScale(ViewZ, ViewZ, 1f);
            View_Rotate = Matrix4.CreateRotationY(0 * 3.14f / 180) * Matrix4.CreateRotationX(0 * 3.14f / 180);
            Vector3 playerPos = new Vector3(ViewX, ViewY, 0);

            foreach (var shader in Program.Shaders.Values)
            {
                shader.Use();
                shader.SetVector3("player_position", playerPos);
                shader.SetMatrix4("view_translate", View_Translate);
                shader.SetMatrix4("view_scale", View_Scale);
                shader.SetMatrix4("view_rotate", View_Rotate);
            }

            GL.BindVertexArray(VertexArrayObject);
            bulletCnt = 0;
            foreach (var obj in SpaceObjects)
            {
                if (obj.ObjectState == SpaceObject.SpaceObjectState.ALIVE)
                {
                    obj.Render(obj.AliveSection, VertexArrayObject);
                }
                else if (obj.ObjectState == SpaceObject.SpaceObjectState.DEAD)
                {
                    obj.Render(obj.DeadSection, VertexArrayObject);
                }
                else
                {
                    obj.Render(VertexArrayObject);
                }
                foreach (var p in obj.Projectiles)
                {
                    bulletCnt += obj.Projectiles.Count;
                    p.Render(VertexArrayObject);
                }
                foreach (var m in obj.Modules)
                {
                    m.Render(VertexArrayObject);
                }
                foreach (var u in obj.UI)
                {
                    u.Render(VertexArrayObject);
                }
            }
            render_sw.Restart();
            foreach (var ui in UIManager.UIGroups.Values)
            {
                if (ui.Enabled)
                {
                    foreach (var obj in ui.GraphicsObjects)
                    {
                        obj.Render(VertexArrayObject);
                    }
                }
            }
            GameCursor.Cursor.Render(VertexArrayObject);

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
