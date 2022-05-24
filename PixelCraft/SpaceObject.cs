using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    public class SpaceObject : RenderObject
    {
        // PHYSICAL
        public float Health = 1;
        public float HealthRegen = 0;
        public float HealthMax = 1;

        public float Armor = 0;
        public float ArmorRegen = 0;
        public float ArmorMax = 0;

        public float Shields = 0;
        public float ShieldRegen = 0;
        public float ShieldMax = 0;

        public float Mass = 0.01f;
        public float CargoMax = 0.5f;

        // BEHAVIOR
        public int Team = 0;
        public bool NPC = true;
        public bool Selected = false;
        public bool Collidable = true;
        public float Aggression = 1;
        public float Morale = 1;
        public float Radius = 1;
        public float SOI = 2;

        // MOVEMENT
        public float Velocity_X = 0;
        public float Velocity_Y = 0;
        public float Acceleration_X = 0;
        public float Acceleration_Y = 0;
        public float Friction = 0;
        public float TopSpeed = 0;
        public float MaxOrbit = 5;
        public float MinOrbit = 2;
        public float Agility = 1;

        // WEAPON
        public bool Armed = false;
        public float FireRate = 1;
        public float Burst = 1;
        public float Accuracy = 1;
        public float Range = 10;
        public Stopwatch FireSW = new Stopwatch();
        public Random WeaponAcc = new Random();

        // PROJECTILE
        public int AmmoCount = 0;
        public float Damage = 0;
        public float ArmorPen = 0;

        // FABRICATION
        public float BuildRate = 1;

        // MINING
        public float MiningRate = 0;

        // STATE
        public enum SpaceObjectState { ALIVE, DEAD, INERT };
        public SpaceObjectState ObjectState = SpaceObjectState.INERT;
        public enum SpaceObjectMvmt { FLYING, LANDED, ANCHORED };
        public SpaceObjectMvmt MvmtState = SpaceObjectMvmt.FLYING;

        // RELATIONSHIPS
        public SpaceObject Target;
        public SpaceObject Host;
        public List<SpaceObject> Modules = new List<SpaceObject>();
        public List<SpaceObject> Materials = new List<SpaceObject>();
        public List<SpaceObject> Attached = new List<SpaceObject>();
        public List<SpaceObject> Projectiles = new List<SpaceObject>();
        public SpaceObject Ammo;
        public List<TextObject> UI = new List<TextObject>();

        public SpaceObject()
        {
            Target = this;
        }

        //public void Detach()
        //{
        //    ObjectState = MvmtState;
        //}

        //public void Attach(SpaceObject host)
        //{
        //    Host = host;
        //    MvmtState = ObjectState;
        //    ObjectState = SpaceObjectState.ANCHORED;
        //}

        public void Update(List<SpaceObject> objs, KeyboardState keybd, GameCursor cursor, double gametime)
        {
            if (Target == null) { Target = this; }
            Health = Math.Min(Health + HealthRegen, HealthMax);
            Shields = Math.Min(Shields + ShieldRegen, ShieldMax);

            for (int i = 0; i < Projectiles.Count; i++)
            {
                var p = Projectiles[i];
                if (p.Distance(new Vector3(0, 0, 0)) > 100)
                {
                    Projectiles.Remove(p);
                }
                else
                {
                    p.Thrust(1, 1);
                }
            }

            SpaceObject inSOI = this;
            SpaceObject inRad = this;
            SpaceObject cursorTarget = this;
            SpaceObject clickTarget = this;
            foreach (var obj in objs)
            {
                if (obj.Collidable && Distance(obj.Position) < obj.SOI && obj != this)
                {
                    inSOI = obj;
                }
                if (Distance(obj.Position) < Radius)
                {
                    inRad = obj;
                }
                if (cursor.Cursor.Distance(obj.Position) < obj.SOI && obj.ObjectState == SpaceObjectState.ALIVE)
                {
                    cursorTarget = obj;
                }

                if (cursor.LeftReleased && Mag(obj.Position.X - cursor.X, obj.Position.Y - cursor.Y) < obj.Radius)
                {
                    clickTarget = obj;
                }

                for (int i = 0; i < Projectiles.Count; i++)
                {
                    var p = Projectiles[i];
                    if (obj.Distance(p.Position) < obj.Radius && obj.Team != Team && obj.Collidable)
                    {
                        if (p.Damage > 0)
                        {
                            float dmg = p.Damage;
                            dmg *= 10 / (10 + obj.Shields);
                            obj.Shields = Math.Max(obj.Shields - Mag(p.Velocity_X, p.Velocity_Y) * 10, 0);
                            //dmg -= obj.Armor;
                            //obj.Armor -= p.ArmorPen;
                            obj.Health -= dmg;
                        }
                        Projectiles.Remove(p);
                    }
                }
            }

            switch (ObjectState)
            {
                case SpaceObjectState.ALIVE:
                    if (Health <= 0) { ObjectState = SpaceObjectState.DEAD; }
                    if (inSOI != this)
                    {
                        this.Flee(inSOI);
                    }
                    else if (NPC)
                    {
                        if (Distance(Target.Position) < MinOrbit) { this.Orbit(Target); }
                        this.Approach(Target);
                        this.Point(Target);
                        foreach (var m in Modules)
                        {
                            m.Target = Target;
                        }
                    }
                    else
                    {
                        this.WASDFly(keybd);
                        foreach (var m in Modules)
                        {
                            //if ((m.Target.NowState == SpaceObjectState.DEAD || m.Target == this) && EnemyAI.Ships.Count > 0)
                            //{
                            //    m.Target = EnemyAI.Ships.First();
                            //}
                            m.Target = cursorTarget;
                        }
                    }
                    break;

                case SpaceObjectState.DEAD:
                    Collidable = false;
                    SOI = 0;
                    foreach (var mod in Modules) { mod.Armed = false; }
                    foreach (var ui in UI) { ui.Visible = false; }
                    if (RenderSections.Count > 1)
                    {
                        RenderSections[0].Visible = false;
                        RenderSections[1].Visible = true;
                    }
                    break;

                default:
                    break;
            }

            foreach (var mod in Modules)
            {
                mod.FireSW.Start();
                if (mod.Armed && mod.Target.ObjectState == SpaceObjectState.ALIVE && Distance(mod.Target.Position) < mod.Range && mod.Ammo != null)
                {
                    if (mod.FireSW.ElapsedMilliseconds > mod.FireRate)
                    {
                        mod.FireSW.Restart();
                        for (int i = 0; i < mod.Burst; i++)
                        {
                            float dacc = (float)(WeaponAcc.NextDouble() - 0.5f) * mod.Accuracy;
                            float vx = Velocity_X;
                            float vy = Velocity_Y;
                            float dx = mod.Target.Position.X - Position.X;
                            float dy = mod.Target.Position.Y - Position.Y;
                            float theta = (float)(Math.Atan(dy / dx) * 180 / Math.PI);
                            if (dx < 0) { theta += 180; }
                            float ax = (dx + dacc) / Distance(mod.Target.Position) * mod.Ammo.TopSpeed;
                            float ay = (dy + dacc) / Distance(mod.Target.Position) * mod.Ammo.TopSpeed;

                            Vector3 pos = Position;
                            Projectiles.Add(new SpaceObject()
                            {
                                Visible = true,
                                RenderSections = mod.Ammo.RenderSections,
                                Shader = mod.Ammo.Shader,
                                Scale = mod.Ammo.Scale,
                                Damage = mod.Ammo.Damage,
                                ArmorPen = mod.Ammo.ArmorPen,
                                Position = pos,
                                Rotation = new Vector3(0, 0, theta - 90),
                                Velocity_X = vx,
                                Velocity_Y = vy,
                                Acceleration_X = ax,
                                Acceleration_Y = ay,
                                TopSpeed = mod.Ammo.TopSpeed
                            });
                            mod.FireSW.Restart();
                        }
                    }
                }
            }

            foreach (var obj in UI)
            {
                obj.SetPosition(Position.X, Position.Y - 1f, Position.Z);
                obj.Text = "HP " + Health.ToString("F0");
            }
        }
    }
}
