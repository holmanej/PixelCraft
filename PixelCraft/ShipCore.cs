using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    class ShipCore : SpaceObject
    {
        public List<SpaceObject> Orbiters;

        public ShipCore()
        {
            Orbiters = new List<SpaceObject>();
        }

        public override void Update(Dictionary<string, GameObject> objs, KeyboardState keybd, GameCursor cursor, double gametime)
        {
            float xMove = 0;
            float yMove = 0;

            if (keybd.IsKeyDown(Key.W)) { yMove = 1; }
            if (keybd.IsKeyDown(Key.S)) { yMove = -1; }
            if (keybd.IsKeyDown(Key.A)) { xMove = -1; }
            if (keybd.IsKeyDown(Key.D)) { xMove = 1; }

            Thrust(xMove, yMove);

            for (int i = 0; i < Orbiters.Count; i++)
            {
                var orb = Orbiters[i];
                if (orb.Anchored)
                {
                    //orb.Seek(Position.X + 2 * (float)Math.Sin(1 / 2 + Math.PI * 2 * Orbiters.IndexOf(orb) / Orbiters.Count), Position.Y + 2 * (float)Math.Cos(1 / 2 + Math.PI * 2 * Orbiters.IndexOf(orb) / Orbiters.Count));
                    orb.Position = new Vector3(Position.X + 2 * (float)Math.Sin(gametime / 2 + Math.PI * 2 * Orbiters.IndexOf(orb) / Orbiters.Count), Position.Y + 2 * (float)Math.Cos(gametime / 2 + Math.PI * 2 * Orbiters.IndexOf(orb) / Orbiters.Count), Position.Z);
                }
                //else
                //{
                //    RemOrbiter(orb);
                //}
            }
            Rotate(0f, 0f, (float)(1.1 * (Math.Sin(gametime) + Math.Sin(gametime * 2.5))));

        }

        public void AddOrbiter(SpaceObject orb)
        {
            Orbiters.Add(orb);
            Ax -= orb.Mass;
            Ay -= orb.Mass;
            orb.Anchored = true;
            orb.Position = Position;
            //orb.Translate(Orbiters.Count, 0, 0);
            //orb.TopSpeed = TopSpeed;
            //orb.Friction = 0.9f;
        }

        public void RemOrbiter(SpaceObject orb)
        {
            Orbiters.Remove(orb);
            Ax += orb.Mass;
            Ay += orb.Mass;
        }
    }
}
