using OpenTK;
using OpenTK.Input;
using System;

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
            obj.Point(obj.Velocity_X, obj.Velocity_Y);
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

        public static void Point(this SpaceObject obj, float dx, float dy)
        {
            float theta = (float)(Math.Atan(dy / dx) * 180 / Math.PI);
            if (dx < 0) { theta += 180; }
            obj.SetRotation(obj.Rotation.X, obj.Rotation.Y, theta - 90);
        }

        public static void Point(this SpaceObject obj, SpaceObject target)
        {
            float dx = target.Position.X - obj.Position.X;
            float dy = target.Position.Y - obj.Position.Y;
            float theta = (float)(Math.Atan(dy / dx) * 180 / Math.PI);
            if (dx < 0) { theta += 180; }
            obj.SetRotation(obj.Rotation.X, obj.Rotation.Y, theta - 90);
        }

        public static void Approach(this SpaceObject obj, SpaceObject target)
        {
            float dx = target.Position.X - obj.Position.X;
            float dy = target.Position.Y - obj.Position.Y;
            float mag = Mag(dx, dy);
            if (mag > obj.MaxOrbit) { obj.Thrust(dx / mag, dy / mag); }
            else { obj.Thrust(0, 0); }
        }

        public static void Orbit(this SpaceObject obj, SpaceObject target)
        {
            float dx = obj.Position.X - target.Position.X;
            float dy = obj.Position.Y - target.Position.Y;
            float mag = Mag(dx, dy);
            obj.Thrust(dx / mag, dy / mag);
            obj.Point(dx, dy);
        }

        public static void Flee(this SpaceObject obj, SpaceObject target)
        {
            float dx = obj.Position.X - target.Position.X;
            float dy = obj.Position.Y - target.Position.Y;
            float mag = Mag(dx, dy);
            obj.Thrust(dx / mag, dy / mag);
            obj.Point(dx, dy);
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