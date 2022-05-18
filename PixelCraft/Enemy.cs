using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PixelCraft
{
    class Enemy : SpaceObject
    {
        public List<SpaceObject> Bullets = new List<SpaceObject>();
        public int BulletsFired = 0;
        Timer FireTimer = new Timer(100);

        public Enemy()
        {
            //Kp = 10;
            //Ki = -4f;
            //Kd = -4f;
            //Ax = 0.005f;
            //Ay = 0.005f;
            //Friction = 0.5f;
            //TopSpeed = 0.05f;
            //FireTimer.Elapsed += FireTimer_Elapsed;
            //FireTimer.Start();
        }

        public void LoadBullets(SpaceObject bullet)
        {
            for (int i = 0; i < 11; i++)
            {
                var b = new SpaceObject() { RenderSections = bullet.RenderSections, Shader = bullet.Shader };
                b.Position = Position;
                b.Translate(0, 0, 0.1f);
                b.Scale = new Vector3(0.1f, 0.1f, 1);
                b.Enabled = false;
                TopSpeed = 20f;
                Bullets.Add(b);
            }
        }

        private void FireTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Random rand = new Random();
            //float z = Rotation.Z + 90 + (BulletsFired - 5) * 2 + rand.Next(-5, 5);
            //Bullets[BulletsFired].TopSpeed = 0.6f;
            //Bullets[BulletsFired].Vx = Vx;
            //Bullets[BulletsFired].Vy = Vy;
            //Bullets[BulletsFired].Ax = (float)Math.Cos(z * 3.14f / 180) * TopSpeed;
            //Bullets[BulletsFired].Ay = (float)Math.Sin(z * 3.14f / 180) * TopSpeed;
            ////Debug.WriteLine(Bullets[BulletsFired].Ax + " " + Bullets[BulletsFired].Ay + " " + z);
            //Bullets[BulletsFired].Enabled = true;
            //if (BulletsFired < 9) { BulletsFired++; FireTimer.Interval = 100; }
            //else if (BulletsFired == 9) { FireTimer.Interval = 1000; BulletsFired++; }
            //else
            //{
            //    foreach (var b in Bullets) { b.Position = Position; b.Vx = 0; b.Vy = 0; }
            //    BulletsFired = 0;
            //}
        }

        public override void Update(Dictionary<string, GameObject> objs, KeyboardState keybd, GameCursor cursor, double gametime)
        {
            var player = (SpaceObject)objs["Player_Core"];
            Seek(player.Position.X + 10f * (float)Math.Sin(gametime * 2), player.Position.Y + 10f * (float)Math.Cos(gametime * 2));
            float dx = player.Position.X - Position.X;
            float dy = player.Position.Y - Position.Y;
            float theta = (float)(Math.Atan(dy / dx) * 180 / Math.PI);
            if (dx < 0) { theta += 180; }
            Rotation = new Vector3(Rotation.X, Rotation.Y, theta - 90);

            //for (int i = 0; i < 10; i++)
            //{
            //    if (i < BulletsFired) { Bullets[i].Thrust(1, 1); }
            //    else { Bullets[i].Position = Position; }
            //}
        }
    }
}
