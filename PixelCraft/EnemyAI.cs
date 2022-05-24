using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    public static class EnemyAI
    {
        static int Team = 2;
        public static Dictionary<string, Shader> Shaders;
        public static Dictionary<string, FontFamily> Fonts;
        public static Dictionary<string, RenderObject.Section> RenderSections;
        static Stopwatch SpawnTimer = new Stopwatch();
        public static List<SpaceObject> Ships = new List<SpaceObject>();

        public static void Update(List<SpaceObject> objs, SpaceObject player)
        {
            SpawnTimer.Start();
            Ships.RemoveAll(i => i.NowState == SpaceObject.SpaceObjectState.DEAD);
            foreach (var ship in Ships) { if (AllyAI.Ships.Count > 0 && ship.Target.NowState == SpaceObject.SpaceObjectState.DEAD) { ship.Target = AllyAI.Ships.Last(); } }
            int shipCount = Ships.Count;
            SpaceObject target = AllyAI.Ships.Count > 0 ? AllyAI.Ships.Last() : player;
            if (shipCount < 3)
            {
                objs.Add(BuildFighter(target));
            }
            if (SpawnTimer.ElapsedMilliseconds > 10000)
            {
                objs.Add(BuildFighter(target));
                SpawnTimer.Restart();
            }
        }

        public static SpaceObject BuildFighter(SpaceObject target)
        {
            var section = new List<RenderObject.Section>();
            section.Add(new RenderObject.Section(RenderSections["Enemy"], true));
            section.Add(new RenderObject.Section(RenderSections["Dednemy"], false));

            Random rand = new Random();
            var enemy = new SpaceObject()
            {
                RenderSections = section,
                Shader = Shaders["texture_shader"],
                Position = new Vector3(rand.Next(-20, 20), rand.Next(-20, 20), 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Health = 50,
                HealthMax = 50,
                TopSpeed = 0.2f,
                Acceleration_X = 0.01f,
                Acceleration_Y = 0.01f,
                Friction = 0.05f,
                SOI = 1f,
                OrbitRange = rand.Next(2, 6),
                NowState = SpaceObject.SpaceObjectState.FLYING,
                Target = target,
                Team = Team
            };
            enemy.UI.Add(new TextObject("HEALTH 9999", Program.FontSets["DebugFont"], Shaders["texture_shader"]) { Position = new Vector3(0f, -0.1f, 0), Scale = new Vector3(0.02f, 0.02f, 1f) });
            enemy.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 4, FireRate = 200, Burst = 3, Target = target });
            enemy.Modules[0].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["RedBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(0.08f, 0.08f, 1f), TopSpeed = 0.3f, Damage = 1 };

            Ships.Add(enemy);
            return enemy;
        }
    }
}
