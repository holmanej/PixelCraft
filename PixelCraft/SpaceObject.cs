using OpenTK;
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
        // F = ma physics time
        public float Vx;
        public float Vy;
        public float Ax = 0.005f;
        public float Ay = 0.005f;
        public float TopSpeed = 0.2f;

        public void Thrust(float x, float y)
        {
            float dx;
            float dy;
            if (x == 0) { dx = Math.Abs(Vx) < Ax ? -Vx : 0.3f * Ax * -Math.Sign(Vx); }
            else { dx = Ax * x; }
            if (y == 0) { dy = Math.Abs(Vy) < Ay ? -Vy : 0.3f * Ay * -Math.Sign(Vy); }
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
    }
}
