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
        static long FighterSpawnTimer = 2000;
        static long GunshipSpawnTimer = 10000;

        public static void Update(List<SpaceObject> objs, SpaceObject player)
        {
            SpawnTimer.Start();
            //Ships.RemoveAll(i => i.ObjectState == SpaceObject.SpaceObjectState.DEAD);
            for (int i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                if (ship.ObjectState == SpaceObject.SpaceObjectState.DEAD)
                {
                    AllyAI.PlayerShip.Score += ship.ScoreValue;
                    Ships.Remove(ship);
                }
                if (AllyAI.Ships.Count > 0 && ship.Target.ObjectState == SpaceObject.SpaceObjectState.DEAD)
                {
                    ship.Target = AllyAI.Ships.Last();
                }
            }
            int shipCount = Ships.Count;
            SpaceObject target = AllyAI.Ships.Count > 0 ? AllyAI.Ships.Last() : player;
            if (shipCount < 3 || SpawnTimer.ElapsedMilliseconds > FighterSpawnTimer)
            {
                objs.Add(BuildFighter(target));
                FighterSpawnTimer += 5000;
            }
            if (SpawnTimer.ElapsedMilliseconds > GunshipSpawnTimer && AllyAI.PlayerShip.Score > 20)
            {
                objs.Add(BuildGunship(target));
                GunshipSpawnTimer = SpawnTimer.ElapsedMilliseconds + 20000;
            }
        }

        public static SpaceObject BuildFighter(SpaceObject target)
        {
            var section = new List<RenderObject.Section>();
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
                ScoreValue = 1,
                MaxOrbit = rand.Next(4, 6),
                MinOrbit = rand.Next(1, 2),
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Target = target,
                Team = Team
            };
            section.Add(new RenderObject.Section(RenderSections["Enemy"], true));
            section.Add(new RenderObject.Section(RenderSections["Dednemy"], false));
            enemy.Modules.Add(new SpaceObject() { Armed = true, FiringArc = 30, Accuracy = 10, FireRate = 200, Burst = 3, Target = target });
            enemy.Modules[0].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["RedBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(0.09f, 0.09f, 1f), TopSpeed = 0.3f, Damage = 1 };

            Ships.Add(enemy);
            return enemy;
        }

        public static SpaceObject BuildGunship(SpaceObject target)
        {
            var section = new List<RenderObject.Section>();
            Random rand = new Random();
            var enemy = new SpaceObject()
            {
                RenderSections = section,
                Shader = Shaders["texture_shader"],
                Position = new Vector3(rand.Next(-20, 20), rand.Next(-20, 20), 0f),
                Scale = new Vector3(2.6f, 2.6f, 1f),
                Health = 250,
                HealthMax = 250,
                TopSpeed = 0.1f,
                Acceleration_X = 0.01f,
                Acceleration_Y = 0.01f,
                Friction = 0.01f,
                Agility = 0.7f,
                SOI = 3f,
                ScoreValue = 5,
                MaxOrbit = 15,
                MinOrbit = 0,
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Target = target,
                Team = Team
            };
            section.Add(new RenderObject.Section(RenderSections["Enemy"], true));
            section.Add(new RenderObject.Section(RenderSections["Dednemy"], false));
            enemy.Modules.Add(new SpaceObject() { Armed = true, Range = 20, FiringArc = 30, Accuracy = 15, FireRate = 40, Burst = 2, Target = target });
            enemy.Modules.Add(new SpaceObject() { Armed = true, Range = 20, FiringArc = 360, FireRate = 2500, Burst = 45, Spread = 360, Target = target });
            enemy.Modules.Add(new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { new RenderObject.Section(RenderSections["Shield"], false) },
                Shader = Shaders["texture_shader"],
                Scale = new Vector3(4f, 4f, 1f),
                Collidable = false,
                ArmorMax = 4,
                Armor = 4
            });
            enemy.Modules[0].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["RedBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(0.2f, 0.2f, 1f), TopSpeed = 0.7f, Damage = 1 };
            enemy.Modules[1].Ammo = new SpaceObject() { RenderSections = new List<RenderObject.Section>() { RenderSections["RedBullet"] }, Shader = Shaders["texture_shader"], Scale = new Vector3(1.0f, 0.5f, 1f), TopSpeed = 0.4f, Damage = 10 };

            Ships.Add(enemy);
            return enemy;
        }
    }
}
