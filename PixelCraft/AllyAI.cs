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
        public static Dictionary<string, Image> Textures;
        static Stopwatch SpawnTimer = new Stopwatch();
        public static List<SpaceObject> Ships = new List<SpaceObject>();

        public static void Update(List<SpaceObject> objs, SpaceObject player)
        {
            SpawnTimer.Start();
            Ships.RemoveAll(i => i.NowState == SpaceObject.SpaceObjectState.DEAD);
            foreach (var ship in Ships) { if (EnemyAI.Ships.Count > 0) { ship.Target = EnemyAI.Ships.Last(); } }
            if (SpawnTimer.ElapsedMilliseconds > 10000 || Ships.Count < EnemyAI.Ships.Count)
            {
                objs.Add(BuildFighter(EnemyAI.Ships.First()));
                SpawnTimer.Restart();
            }
        }

        public static SpaceObject BuildCore()
        {
            var core = new SpaceObject()
            {
                RenderSections = Program.Img2Sect(Textures["ShipCore"]),
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

            core.RenderSections.Add(new RenderObject.Section(Textures["ShipCore_Dead"]) { Visible = false });
            core.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 4, FireRate = 200, Burst = 2, Target = core });
            core.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 4, FireRate = 200, Burst = 2, Target = core });
            core.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 1, FireRate = 1000, Burst = 1, Target = core });
            core.Modules[0].Ammo = new SpaceObject() { RenderSections = Program.Img2Sect(Textures["GreenBullet"], Textures["BulletHit"]), Shader = Shaders["texture_shader"], Scale = new Vector3(0.1f, 0.1f, 1f), TopSpeed = 0.4f, Damage = 0 };
            core.Modules[1].Ammo = new SpaceObject() { RenderSections = Program.Img2Sect(Textures["GreenBullet"], Textures["BulletHit"]), Shader = Shaders["texture_shader"], Scale = new Vector3(0.1f, 0.1f, 1f), TopSpeed = 0.4f, Damage = 0 };
            core.Modules[2].Ammo = new SpaceObject() { RenderSections = Program.Img2Sect(Textures["GreenBullet"], Textures["BulletHit"]), Shader = Shaders["texture_shader"], Scale = new Vector3(0.2f, 0.9f, 1f), TopSpeed = 0.8f, Damage = 0 };
            core.UI.Add(new TextObject("HEALTH 9999", Fonts["times"], Shaders["texture_shader"]) { Position = new Vector3(0f, 0f, 0), Scale = new Vector3(5f, 5f, 0f), Color = Color.White, BGColor = Color.Black, Size = 24 });
            Ships.Add(core);

            return core;
        }

        public static SpaceObject BuildFighter(SpaceObject target)
        {
            Random rand = new Random();
            var ally = new SpaceObject()
            {
                RenderSections = Program.Img2Sect(Textures["Ally"]),
                Shader = Shaders["texture_shader"],
                Position = new Vector3(rand.Next(-10, 10), rand.Next(-10, 10), 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Health = 250,
                HealthMax = 250,
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
            ally.UI.Add(new TextObject("HEALTH 9999", Fonts["times"], Shaders["texture_shader"]) { Position = new Vector3(0f, 0f, 0), Scale = new Vector3(5f, 5f, 0f), Color = Color.White, BGColor = Color.Black, Size = 24 });
            ally.RenderSections.Add(new RenderObject.Section(Textures["Ally_Dead"]) { Visible = false });
            ally.Modules.Add(new SpaceObject() { Armed = true, Accuracy = 1, FireRate = 100, Burst = 1, Target = target });
            ally.Modules[0].Ammo = new SpaceObject() { RenderSections = Program.Img2Sect(Textures["BlueBullet"], Textures["BulletHit"]), Shader = Shaders["texture_shader"], Scale = new Vector3(0.08f, 0.08f, 1f), TopSpeed = 0.5f, Damage = 1 };

            Ships.Add(ally);
            return ally;
        }
    }
}
