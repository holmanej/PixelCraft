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
    public static class AllyAI
    {
        public static SpaceObject PlayerShip;
        static int Team = 1;
        public static Dictionary<string, Shader> Shaders;
        public static Dictionary<string, FontFamily> Fonts;
        public static Dictionary<string, RenderObject.Section> RenderSections;
        static Stopwatch SpawnTimer = new Stopwatch();
        static Random Rand = new Random();
        public static List<SpaceObject> Ships = new List<SpaceObject>();
        public static int FighterSpawnRate = 3000;
        public static int TankSpawnRate = 10000;

        public static void Update(List<SpaceObject> objs)
        {
            SpawnTimer.Start();
            for (int i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                if (ship.ObjectState == SpaceObject.SpaceObjectState.DEAD) { Ships.Remove(ship); }
                if (EnemyAI.Ships.Count > 0 && (ship.Target.ObjectState == SpaceObject.SpaceObjectState.DEAD || ship.Target == ship))
                {
                    ship.Target = EnemyAI.Ships.First();
                }
            }
            if (SpawnTimer.ElapsedMilliseconds > FighterSpawnRate || Ships.Count == 0)
            {
                objs.Add(BuildFighter(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                FighterSpawnRate += 8000;
            }
            if (SpawnTimer.ElapsedMilliseconds > TankSpawnRate || Ships.Count == 0)
            {
                objs.Add(BuildTank(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                TankSpawnRate += 30000;
            }
            if (!Ships.Exists(s => s.NPC == false))
            {
                Debug.WriteLine("Respawn");
                int score = PlayerShip == null ? 0 : PlayerShip.Score;
                PlayerShip = BuildCore();
                PlayerShip.Score = 100 + score;
                objs.Add(PlayerShip);
            }
        }

        public static SpaceObject BuildCore()
        {
            var section = new List<RenderObject.Section>();
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
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Health = 100,
                HealthMax = 100,
                HealthRegen = 0.005f,
            };
            section.Add(new RenderObject.Section(RenderSections["ShipCore"], true));
            section.Add(new RenderObject.Section(RenderSections["ShipCore_Dead"], false));            
            core.Modules.Add(new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { new RenderObject.Section(RenderSections["Shield"], true) },
                Shader = Shaders["texture_shader"],
                Scale = new Vector3(1.5f, 1.5f, 1f),
                Collidable = false,
                ShieldMax = 100, ShieldRegen = 0.1f, Shields = 100
            });
            core.Modules.Add(GunTypes.FPGun(AmmoTypes.MDAmmo()));
            core.Modules.Add(GunTypes.GRGun(AmmoTypes.MSAmmo()));
            Ships.Add(core);

            return core;
        }

        public static SpaceObject BuildFighter(int x, int y)
        {
            var section = new List<RenderObject.Section>();
            var ally = new SpaceObject()
            {
                RenderSections = section,
                Shader = Shaders["texture_shader"],
                Position = new Vector3(x, y, 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Health = 25,
                HealthMax = 25,
                TopSpeed = 0.2f,
                Acceleration_X = 0.01f,
                Acceleration_Y = 0.01f,
                Friction = 0.05f,
                SOI = 1f,
                MaxOrbit = 4,
                MinOrbit = 0,
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Team = Team
            };
            section.Add(new RenderObject.Section(RenderSections["Ally"], true));
            section.Add(new RenderObject.Section(RenderSections["Ally_Dead"], false));
            ally.Modules.Add(GunTypes.GRGun(AmmoTypes.MSAmmo()));
            
            Ships.Add(ally);
            return ally;
        }

        public static SpaceObject BuildTank(int x, int y)
        {
            var section = new List<RenderObject.Section>();
            Random rand = new Random();
            var ally = new SpaceObject()
            {
                RenderSections = section,
                Shader = Shaders["texture_shader"],
                Position = new Vector3(x, y, 0f),
                Scale = new Vector3(3f, 1f, 1f),
                Health = 100,
                HealthMax = 100,
                TopSpeed = 0.2f,
                Acceleration_X = 0.01f,
                Acceleration_Y = 0.01f,
                Friction = 0.05f,
                Agility = 3f,
                SOI = 1f,
                MaxOrbit = 4,
                MinOrbit = 0,
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Team = Team
            };
            section.Add(new RenderObject.Section(RenderSections["Ally"], true));
            section.Add(new RenderObject.Section(RenderSections["Ally_Dead"], false));
            ally.Modules.Add(GunTypes.FBGun(AmmoTypes.SDAmmo()));
            ally.Modules.Add(new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { new RenderObject.Section(RenderSections["Shield"], true) },
                Shader = Shaders["texture_shader"],
                Scale = new Vector3(4f, 4f, 1f),
                Collidable = false,
                ShieldMax = 200, ShieldRegen = 0.05f, Shields = 200
            });

            Ships.Add(ally);
            return ally;
        }
    }
}
