﻿using OpenTK;
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
        public static GameWindow Gwin;
        static List<Spawner> Spawners = new List<Spawner>();
        public static Action LevelAI;
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

        public static void ChangeLevel(string level)
        {
            Spawners.Clear();
            AllyAI.Ships.Clear();
            EnemyAI.Ships.Clear();
            Gwin.SpaceObjects.Clear();
            SpawnTimer.Restart();

            switch (level)
            {
                case "Title": Title(); break;
                case "GunTest": GunTest(); break;
                case "Arcade": Arcade(); break;
                default: break;
            }
        }

        public static void Title()
        {
            AllyAI.PlayerShip = new SpaceObject();
            UIManager.UIGroups["UpgradePanel"].Enabled = false;
            UIManager.UIGroups["Title"].Enabled = true;
            LevelAI = new Action(() => { });
        }

        public static void GunTest()
        {
            UIManager.UIGroups["Title"].Enabled = false;
            var worldObjs = new List<SpaceObject>();
            worldObjs.Add(new SpaceObject() { RenderSections = new List<Section>() { Program.RenderSections["StarField"] }, Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(0f, 0f, 0.2f),
                Scale = new Vector3(50f, 50f, 1f),
                Collidable = false
            });
            Gwin.SpaceObjects.AddRange(worldObjs);

            LevelAI = new Action(() =>
            {
                if (EnemyAI.Ships.Count == 0)
                {
                    var fighter = EnemyAI.BuildFighter(5, 0);
                    fighter.TopSpeed = 0;
                    Gwin.SpaceObjects.Add(fighter);
                }
                if (!AllyAI.Ships.Exists(s => s.NPC == false))
                {
                    Debug.WriteLine("Respawn");
                    int score = AllyAI.PlayerShip == null ? 0 : AllyAI.PlayerShip.Score;
                    AllyAI.PlayerShip = AllyAI.BuildCore();
                    AllyAI.PlayerShip.Score = 100 + score;
                    Gwin.SpaceObjects.Add(AllyAI.PlayerShip);
                }
            });

            UIManager.UIGroups["UpgradePanel"].Enabled = true;
            SpawnTimer.Restart();
        }

        public static void Arcade()
        {
            UIManager.UIGroups["Title"].Enabled = false;
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
            Gwin.SpaceObjects.AddRange(worldObjs);

            // ENEMY
            Spawners.Add(new Spawner(0, 2000));         // FIGHTER
            Spawners.Add(new Spawner(45000, 30000));    // GUNSHIP

            // ALLY
            Spawners.Add(new Spawner(5000, 4000));      // FIGHTER
            Spawners.Add(new Spawner(40000, 30000));    // TANK

            LevelAI = new Action(() =>
            {
                if (Spawners[0].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    Gwin.SpaceObjects.Add(EnemyAI.BuildFighter(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (Spawners[1].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    Gwin.SpaceObjects.Add(EnemyAI.BuildGunship(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (Spawners[2].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    Gwin.SpaceObjects.Add(AllyAI.BuildFighter(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (Spawners[3].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    Gwin.SpaceObjects.Add(AllyAI.BuildTank(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                }
                if (!AllyAI.Ships.Exists(s => s.NPC == false))
                {
                    Debug.WriteLine("Respawn");
                    int score = AllyAI.PlayerShip == null ? 0 : AllyAI.PlayerShip.Score;
                    AllyAI.PlayerShip = AllyAI.BuildCore();
                    AllyAI.PlayerShip.Score = 100 + score;
                    Gwin.SpaceObjects.Add(AllyAI.PlayerShip);
                }
            });

            UIManager.UIGroups["UpgradePanel"].Enabled = true;
            SpawnTimer.Restart();
        }
    }
}
