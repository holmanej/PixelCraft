using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    class SpaceObject : GameObject
    {
        public float Vx;
        public float Vy;
        public float Ax = 0.005f;
        public float Ay = 0.005f;
        public float Friction = 0.3f;
        public float TopSpeed = 0.2f;
        public float Mass = 0.001f;
        public float SOI = 1;
        public bool Anchored = true;

        public bool Selected = false;

        // Movement PID
        float Kp = 0.1f;
        float Ki = 0.04f;
        float Kd = 0.01f;
        float Zx_1;
        float Zy_1;
        float Zx_2;
        float Zy_2;

        public void Thrust(float x, float y)
        {
            float dx;
            float dy;
            if (x == 0) { dx = Math.Abs(Vx) < Ax ? -Vx / 5 : Friction * Ax * -Math.Sign(Vx); }
            else { dx = Ax * x; }
            if (y == 0) { dy = Math.Abs(Vy) < Ay ? -Vy / 5 : Friction * Ay * -Math.Sign(Vy); }
            else { dy = Ay * y; }

            float dt = (float)Math.Sqrt(Math.Pow(Vx + dx, 2) + Math.Pow(Vy + dy, 2));
            if (dt < TopSpeed)
            {
                Vx += dx;
                Vy += dy;
            }
            else if (dt > TopSpeed)
            {
                Vx = (Vx + dx) / dt * TopSpeed;                
                Vy = (Vy + dy) / dt * TopSpeed;
            }

            Translate(Vx, Vy, 0);
        }

        public void Seek(float x, float y)
        {
            //float dist = Vector3.Distance(Position, new Vector3(x, y, Position.Z));
            float Zx_0 = x - Position.X;
            float Zy_0 = y - Position.Y;
            float dx = (Zx_0 * Kp + Zx_1 * Ki + Zx_2 * Kd) / 3;
            float dy = (Zy_0 * Kp + Zy_1 * Ki + Zy_2 * Kd) / 3;
            float dist = (float)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            if (dist > SOI) { Thrust(dx / dist, dy / dist); }
            else { Thrust(0, 0); }
            //Debug.WriteLine(dx / dist + " " + dy / dist);

            Zx_2 = Zx_1;
            Zy_2 = Zy_1;
            Zx_1 = Zx_0;
            Zy_1 = Zy_0;
        }

        public override void Update(Dictionary<string, GameObject> objs, KeyboardState keybd, GameCursor cursor, double gametime)
        {
            var pcore = (ShipCore)objs["Player_Core"];
            var roid = (Asteroid)objs["Asteroid"];
            Vector3 cpos = new Vector3(cursor.X, cursor.Y, 0);
            float dist = Vector3.Distance(Position, cpos);
            //Debug.WriteLine(dist + " : " + Position.X + " " + Position.Y + " : " + cursor.X + " " + cursor.Y);
            
            if (cursor.LeftPressed)
            {
                if (dist < SOI)
                {
                    Selected = true;
                }
                else
                {
                    dist = Vector3.Distance(cpos, roid.Position);
                    if (Selected && dist < roid.SOI)
                    {
                        Anchored = true;
                        pcore.RemOrbiter(this);
                        roid.Orbiters.Add(this);
                        float x = (cursor.X - roid.Position.X) / dist;
                        float y = (cursor.Y - roid.Position.Y) / dist;
                        Rotation = new Vector3(Rotation.X, Rotation.Y, (float)(Math.Atan(y / x) * 180 / Math.PI) - 90);
                        x *= roid.SOI + 0.1f;
                        y *= roid.SOI + 0.1f;
                        Position = new Vector3(x, y, -0.9f);
                    }
                    Selected = false;
                }
            }
            //Debug.WriteLine(Selected);

            dist = Vector3.Distance(pcore.Position, Position);
            if (dist < pcore.SOI && !Anchored)
            {
                pcore.AddOrbiter(this);
            }
        }
    }
}
