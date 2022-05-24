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
    class AllyAI
    {
        static int Team = 1;
        public static Dictionary<string, Shader> Shaders;
        public static Dictionary<string, FontFamily> Fonts;
        public static Dictionary<string, RenderObject.Section> RenderSections;
        static Stopwatch SpawnTimer = new Stopwatch();
        public static List<SpaceObject> Ships = new List<SpaceObject>();

        public static void Update(List<SpaceObject> objs, SpaceObject player)
        {
            SpawnTimer.Start();
            Ships.RemoveAll(i => i.NowState == SpaceObject.SpaceObjectState.DEAD);
            foreach (var ship in Ships) { if (EnemyAI.Ships.Count > 0) { ship.Target = EnemyAI.Ships.First(); } }
            if (SpawnTimer.ElapsedMilliseconds > 10000 || Ships.Count < EnemyAI.Ships.Count)
            {
                objs.Add(BuildFighter(EnemyAI.Ships.First()));
                SpawnTimer.Restart();
            }
        }

        public static SpaceObject BuildCore()
        {
            var section = new List<RenderObject.Section>();
            section.Add(new RenderObject.Section(RenderSections["ShipCore"], true));
            section.Add(new RenderObject.Section(RenderSections["ShipCore_Dead"], false));

            var core = new SpaceObject()
            {
                RenderSections = section,
                Shader = Shaders["texture_shader"],
                Position = new Vector3(0f, 0f, 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Team = 1,
                NPC = false,
                TopSpeed = 0.2f,
                Acceleration_X = 0.005f,
                Acceleration_Y = 0.005f,
                Friction = 0.3f,
                SOI = 1f,
                NowState = SpaceObject.SpaceObjectState.FLYING,
                Health = 1000,
                HealthMax = 10000,
                HealthRegen = 0.8f
            };

            core.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 4, FireRate = 200, Burst = 2, Target = core });
            core.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 4, FireRate = 200, Burst = 2, Target = core });
            core.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 1, FireRate = 1000, Burst = 1, Target = core });
            core.Modules[0].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["GreenBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(0.1f, 0.1f, 1f), TopSpeed = 0.4f, Damage = 0 };
            core.Modules[1].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["GreenBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(0.1f, 0.1f, 1f), TopSpeed = 0.4f, Damage = 0 };
            core.Modules[2].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["GreenBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(0.2f, 0.9f, 1f), TopSpeed = 0.8f, Damage = 0 };
            core.UI.Add(new TextObject("HEALTH 9999", Program.FontSets["DebugFont"], Shaders["texture_shader"]) { Position = new Vector3(0f, -0.1f, 0), Scale = new Vector3(0.02f, 0.02f, 1f) });
            Ships.Add(core);

            return core;
        }

        public static SpaceObject BuildFighter(SpaceObject target)
        {
            var section = new List<RenderObject.Section>();
            section.Add(new RenderObject.Section(RenderSections["Ally"], true));
            section.Add(new RenderObject.Section(RenderSections["Ally_Dead"], false));

            
            Random rand = new Random();
            var ally = new SpaceObject()
            {
                RenderSections = section,
                Shader = Shaders["texture_shader"],
                Position = new Vector3(rand.Next(-20, 20), rand.Next(-20, 20), 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Health = 25,
                HealthMax = 25,
                TopSpeed = 0.2f,
                Acceleration_X = 0.01f,
                Acceleration_Y = 0.01f,
                Friction = 0.05f,
                SOI = 1f,
                OrbitRange = rand.Next(5, 10),
                NowState = SpaceObject.SpaceObjectState.FLYING,
                Target = target,
                Team = Team
            };

            ally.UI.Add(new TextObject("HEALTH 9999", Program.FontSets["DebugFont"], Shaders["texture_shader"]) { Position = new Vector3(0f, -0.1f, 0), Scale = new Vector3(0.02f, 0.02f, 1f) });
            ally.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 1, FireRate = 100, Burst = 1, Target = target });
            ally.Modules[0].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["BlueBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(0.08f, 0.08f, 1f), TopSpeed = 0.5f, Damage = 1 };
            
            Ships.Add(ally);
            return ally;
        }
    }
}
