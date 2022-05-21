using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    class Asteroid : SpaceObject
    {
        public List<SpaceObject> Orbiters = new List<SpaceObject>();
        float Spin = -0.002f;

        public override void Update(Dictionary<string, SpaceObject> objs, KeyboardState keybd, GameCursor cursor, double gametime)
        {
            //Rotate(0f, 0f, Spin);
            //foreach (var orb in Orbiters)
            //{
            //    Vector3 v = Vector3.Cross(Rotation, new Vector3(orb.Position.X - Position.X, orb.Position.Y - Position.Y, 0));
            //    orb.Translate(v.X, v.Y, 0);
            //}
        }
    }
}
