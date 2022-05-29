using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelCraft.RenderObject;

namespace PixelCraft
{
    class WorldManager
    {
        public static List<SpaceObject> CurrentLevel = new List<SpaceObject>();
        static List<Spawner> Spawners = new List<Spawner>();
        public static Delegate LevelBehavior;
        static Random Rand = new Random();
        static Stopwatch SpawnTimer = new Stopwatch();

        class Spawner
        {
            long IncTime;
            long NextTime;

            public Spawner(long init, long inc)
            {
                IncTime = inc;
                NextTime = init;
            }

            public bool Update(long time)
            {
                if (time > NextTime)
                {
                    NextTime += IncTime;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static List<SpaceObject> Level0()
        {
            var worldObjs = new List<SpaceObject>();
            worldObjs.Add(new SpaceObject() { RenderSections = new List<Section>() { Program.RenderSections["StarField"] }, Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(0f, 0f, 0.2f),
                Scale = new Vector3(50f, 50f, 1f),
                Collidable = false
            });

            LevelBehavior = new Action<List<SpaceObject>>((spaceObjs) =>
            {
                if (EnemyAI.Ships.Count == 0)
                {
                    var fighter = EnemyAI.BuildFighter(5, 0);
                    fighter.TopSpeed = 0;
                    spaceObjs.Add(fighter);
                }
                if (!AllyAI.Ships.Exists(s => s.NPC == false))
                {
                    Debug.WriteLine("Respawn");
                    int score = AllyAI.PlayerShip == null ? 0 : AllyAI.PlayerShip.Score;
                    AllyAI.PlayerShip = AllyAI.BuildCore();
                    AllyAI.PlayerShip.Score = 100 + score;
                    spaceObjs.Add(AllyAI.PlayerShip);
                }
            });

            SpawnTimer.Restart();
            return worldObjs;
        }

        public static List<SpaceObject> Level1()
        {
            var worldObjs = new List<SpaceObject>();
            worldObjs.Add(new SpaceObject()
            {
                RenderSections = new List<Section>() { Program.RenderSections["StarField"] },
                Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(0f, 0f, 0.2f),
                Scale = new Vector3(50f, 50f, 1f),
                Collidable = false
            });

            Random rand = new Random();
            List<SpaceObject> asteroids = new List<SpaceObject>();
            for (int i = 0; i < 15; i++)
            {
                int x = rand.Next(-50, 50);
                int y = rand.Next(-50, 50);
                int size = rand.Next(1, 6) / 2;
                asteroids.Add(new SpaceObject()
                {
                    RenderSections = new List<Section>() { Program.RenderSections["asteroid"] },
                    Shader = Program.Shaders["texture_shader"],
                    Position = new Vector3(x, y, 0.1f),
                    Scale = new Vector3(size, size, 1f),
                    Rotation = new Vector3(0, 0, 0),
                    Radius = size,
                    SOI = size * 1.5f,
                });
            }
            worldObjs.AddRange(asteroids);

            // ENEMY
            Spawners.Add(new Spawner(0, 2000));         // FIGHTER
            Spawners.Add(new Spawner(45000, 30000));    // GUNSHIP

            // ALLY
            Spawners.Add(new Spawner(5000, 4000));      // FIGHTER
            Spawners.Add(new Spawner(40000, 30000));    // TANK

            LevelBehavior = new Action<List<SpaceObject>>((spaceObjs) =>
            {
                if (Spawners[0].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    spaceObjs.Add(EnemyAI.BuildFighter(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (Spawners[1].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    spaceObjs.Add(EnemyAI.BuildGunship(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (Spawners[2].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    spaceObjs.Add(AllyAI.BuildFighter(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (Spawners[3].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    spaceObjs.Add(AllyAI.BuildTank(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (!AllyAI.Ships.Exists(s => s.NPC == false))
                {
                    Debug.WriteLine("Respawn");
                    int score = AllyAI.PlayerShip == null ? 0 : AllyAI.PlayerShip.Score;
                    AllyAI.PlayerShip = AllyAI.BuildCore();
                    AllyAI.PlayerShip.Score = 100 + score;
                    spaceObjs.Add(AllyAI.PlayerShip);
                }
            });

            SpawnTimer.Restart();

            return worldObjs;
        }
    }
}
