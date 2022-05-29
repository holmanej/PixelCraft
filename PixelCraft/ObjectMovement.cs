using OpenTK;
using OpenTK.Input;
using System;
using System.Diagnostics;

namespace PixelCraft
{
    public static class ObjectMovement
    {
        public static void WASDFly(this SpaceObject obj, KeyboardState keybd)
        {
            float xMove = 0;
            float yMove = 0;

            if (keybd.IsKeyDown(Key.W)) { yMove = 1; }
            if (keybd.IsKeyDown(Key.S)) { yMove = -1; }
            if (keybd.IsKeyDown(Key.A)) { xMove = -1; }
            if (keybd.IsKeyDown(Key.D)) { xMove = 1; }

            obj.Thrust(xMove, yMove);
        }

        public static void Thrust(this SpaceObject obj, float x, float y)
        {
            float Vx = obj.Velocity_X;
            float Vy = obj.Velocity_Y;
            float Ax = obj.Acceleration_X;
            float Ay = obj.Acceleration_Y;
            float TopSpeed = obj.TopSpeed;
            float mu = obj.Friction;
            float dx;
            float dy;
            if (x == 0) { dx = Math.Abs(Vx) < Ax ? -Vx / 5 : mu * Ax * -Math.Sign(Vx); }
            else { dx = Ax * x; }
            if (y == 0) { dy = Math.Abs(Vy) < Ay ? -Vy / 5 : mu * Ay * -Math.Sign(Vy); }
            else { dy = Ay * y; }

            float dt = Mag(Vx + dx, Vy + dy);
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

            obj.Velocity_X = Vx;
            obj.Velocity_Y = Vy;
            obj.Translate(Vx, Vy, 0);
        }

        public static void Point(this SpaceObject obj, float x, float y)
        {
            if (x != 0)
            {
                //float theta = (float)Math.Atan(dy / dx);
                //if (dx < 0) { theta += 3.14f; }
                //obj.SetRotation(obj.Rotation.X, obj.Rotation.Y, theta * 180f / 3.14f - 90);
                float dt = obj.dTheta(new Vector3(x, y, 0));
                float theta = Math.Abs(dt) < obj.Agility ? dt : Math.Sign(dt) * obj.Agility;
                obj.Rotate(0, 0, theta);
            }
        }

        public static void Point(this SpaceObject obj, SpaceObject target)
        {
            if (obj != target)
            {
                float dt = obj.dTheta(target.Position);
                float theta = Math.Abs(dt) < obj.Agility ? dt : Math.Sign(dt) * obj.Agility;
                obj.Rotate(0, 0, theta);
            }
        }

        public static void Approach(this SpaceObject obj, SpaceObject target)
        {
            if (obj != target)
            {
                float dx = target.Position.X - obj.Position.X;
                float dy = target.Position.Y - obj.Position.Y;
                float mag = Mag(dx, dy);
                if (mag > obj.MaxOrbit) { obj.Thrust(dx / mag, dy / mag); }
                else { obj.Thrust(0, 0); }
            }
        }

        public static void Orbit(this SpaceObject obj, SpaceObject target)
        {
            if (obj != target)
            {
                float dx = obj.Position.X - target.Position.X;
                float dy = obj.Position.Y - target.Position.Y;
                float mag = Mag(dx, dy);
                obj.Thrust(dx / mag, dy / mag);
                obj.Point(dx, dy);
            }
        }

        public static void Flee(this SpaceObject obj, SpaceObject target)
        {
            if (obj.Position == target.Position)
            {
                Random rand = new Random();
                obj.Thrust((float)rand.NextDouble(), (float)rand.NextDouble());
            }
            float dx = obj.Position.X - target.Position.X;
            float dy = obj.Position.Y - target.Position.Y;
            float mag = Mag(dx, dy);
            obj.Thrust(dx / mag, dy / mag);
        }

        public static void Walk(this SpaceObject obj)
        {
            
        }

        public static void Jump(this SpaceObject obj)
        {

        }

        public static float Mag(float a, float b)
        {
            return (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
    }
}