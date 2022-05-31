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
        static string CurrentLevel;
        static Random Rand = new Random();
        public static Stopwatch SpawnTimer = new Stopwatch();

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
            UIManager.UIGroups["WinScreen"].Enabled = false;
            UIManager.UIGroups["WinScreen"].GraphicsObjects[0].SetAlpha(0);
            UIManager.UIGroups["DeathScreen"].Enabled = false;
            UIManager.UIGroups["DeathScreen"].GraphicsObjects[0].SetAlpha(0);
            UIManager.UIGroups["Countdown"].Enabled = false;
            Spawners.Clear();
            AllyAI.Ships.Clear();
            EnemyAI.Ships.Clear();
            Gwin.SpaceObjects.Clear();
            SpawnTimer.Restart();
            CurrentLevel = level;

            switch (level)
            {
                case "Title": Title(); break;
                case "GunTest": GunTest(); break;
                case "Arcade": Arcade(); break;
                case "Peaceful": Peaceful(); break;
                default: break;
            }
        }

        public static void RestartLevel()
        {
            UIManager.UIGroups["DeathScreen"].Enabled = false;
            ChangeLevel(CurrentLevel);
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
                    fighter.Modules.Clear();
                    Gwin.SpaceObjects.Add(fighter);
                }
                if (!AllyAI.Ships.Exists(s => s.NPC == false))
                {
                    Debug.WriteLine("Respawn");
                    AllyAI.PlayerShip = AllyAI.BuildCore();
                    AllyAI.PlayerShip.Score = 100000;
                    Gwin.SpaceObjects.Add(AllyAI.PlayerShip);
                }
            });

            Gwin.ViewZ = 0.06f;
            UIManager.UIGroups["Title"].Enabled = false;
            UIManager.UIGroups["UpgradePanel"].Enabled = true;
            SpawnTimer.Restart();
        }

        public static void Peaceful()
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
            worldObjs.Add(new SpaceObject()
            {
                RenderSections = new List<Section>() { Program.RenderSections["asteroid"] },
                Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(15, 0, 0.1f),
                Scale = new Vector3(25, 25, 1f),
                Rotation = new Vector3(0, 0, 0),
                Radius = 25,
                SOI = 28,
            });
            Gwin.SpaceObjects.AddRange(worldObjs);

            LevelAI = new Action(() =>
            {
                if (!AllyAI.Ships.Exists(s => s.NPC == false))
                {
                    Debug.WriteLine("Respawn");
                    AllyAI.PlayerShip = AllyAI.BuildCore();
                    AllyAI.PlayerShip.Score = 100000;
                    Gwin.SpaceObjects.Add(AllyAI.PlayerShip);
                }
            });

            Gwin.ViewZ = 0.06f;
            UIManager.UIGroups["Title"].Enabled = false;
            UIManager.UIGroups["UpgradePanel"].Enabled = true;
            SpawnTimer.Restart();
        }

        public static void Arcade()
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
            Gwin.SpaceObjects.AddRange(worldObjs);

            // ENEMY
            Spawners.Add(new Spawner(20000, 2000));     // FIGHTER
            Spawners.Add(new Spawner(65000, 30000));    // GUNSHIP

            // ALLY
            Spawners.Add(new Spawner(24000, 6000));     // FIGHTER
            Spawners.Add(new Spawner(60000, 35000));    // TANK

            LevelAI = new Action(() =>
            {
                if (Spawners[0].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    for (int i = EnemyAI.Roster["fighter"]; i < SpawnTimer.ElapsedMilliseconds / 45000 + 3; i++)
                    {
                        Gwin.SpaceObjects.Add(EnemyAI.BuildFighter(AllyAI.PlayerShip.Position.X - 20, Rand.Next(-20, 20)));
                    }
                }
                if (Spawners[1].Update(SpawnTimer.ElapsedMilliseconds) && AllyAI.Roster["tank"] < 2)
                {
                    Gwin.SpaceObjects.Add(EnemyAI.BuildGunship(Rand.Next(-5, 5), Rand.Next(-5, 5)));
                    for (int i = EnemyAI.Roster["gunship"]; i < SpawnTimer.ElapsedMilliseconds / 75000; i++)
                    {
                        Gwin.SpaceObjects.Add(EnemyAI.BuildGunship(Rand.Next(-20, 20), Rand.Next(-20, 20)));
                    }
                }
                if (Spawners[2].Update(SpawnTimer.ElapsedMilliseconds) && AllyAI.Roster["fighter"] < 5)
                {
                    Gwin.SpaceObjects.Add(AllyAI.BuildFighter(Rand.Next(-20, 20), -20));
                }
                if (Spawners[3].Update(SpawnTimer.ElapsedMilliseconds))
                {
                    Gwin.SpaceObjects.Add(AllyAI.BuildTank(Rand.Next(-20, 20), -20));
                }
                if (!AllyAI.Ships.Exists(s => s.NPC == false))
                {
                    Debug.WriteLine("Respawn");
                    AllyAI.PlayerShip = AllyAI.BuildCore();
                    AllyAI.PlayerShip.Score = 200;
                    Gwin.SpaceObjects.Add(AllyAI.PlayerShip);
                }
                else if (AllyAI.PlayerShip.ObjectState == SpaceObject.SpaceObjectState.DEAD)
                {
                    UIManager.UIGroups["WinScreen"].Enabled = false;
                    UIManager.UIGroups["DeathScreen"].Enabled = true;
                }
                if (SpawnTimer.ElapsedMilliseconds > 320000 && AllyAI.PlayerShip.ObjectState == SpaceObject.SpaceObjectState.ALIVE)
                {
                    UIManager.UIGroups["WinScreen"].Enabled = true;
                }
            });

            Gwin.ViewZ = 0.06f;
            UIManager.UIGroups["Title"].Enabled = false;
            UIManager.UIGroups["UpgradePanel"].Enabled = true;
            UIManager.UIGroups["Countdown"].Enabled = true;
            SpawnTimer.Restart();
        }
    }
}
