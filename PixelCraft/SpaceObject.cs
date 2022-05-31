using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
        // META
        public string Type = "none";
        private Section _AliveSection;
        private Section _DeadSection;
        public Section AliveSection
        {
            get { return _AliveSection; }
            set { _AliveSection = value; RenderSections.Add(value); }
        }
        public Section DeadSection
        {
            get { return _DeadSection; }
            set { _DeadSection = value; RenderSections.Add(value); }
        }

        // PHYSICAL
        public float Health = 0;
        public float HealthRegen = 0;
        public float HealthMax = 0;

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
        public int ScoreValue = 1;
        public int Score = 0;

        // MOVEMENT
        public float Velocity_X = 0;
        public float Velocity_Y = 0;
        public float Acceleration_X = 0;
        public float Acceleration_Y = 0;
        public float Friction = 0;
        public float TopSpeed = 0;
        public float MaxOrbit = 5;
        public float MinOrbit = 2;
        public float Agility = 2f;

        // WEAPON
        public bool Armed = false;
        public float FireRate = float.MaxValue;
        public float FiringArc = 15;
        public float Burst = 0;
        public float Spread = 0;
        public float Accuracy = 0;
        public float Range = 10;
        public Stopwatch FireSW = new Stopwatch();
        public Stopwatch DespawnSW = new Stopwatch();
        public Random WeaponAcc = new Random();

        // PROJECTILE
        public float Damage = 0;
        public float ArmorPen = 0;

        // FABRICATION
        public float BuildRate = 1;

        // MINING
        public float MiningRate = 0;

        // STATE
        public enum SpaceObjectState { ALIVE, DEAD, INERT, TBD };
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
            UI.Add(new TextObject("HEALTH 9999", Program.FontSets["DebugFont"], Program.Shaders["texture_shader"]) { Scale = new Vector3(0.02f, 0.02f, 1f) });
            UI.Add(new TextObject("ARMOR 9999", Program.FontSets["DebugFont"], Program.Shaders["texture_shader"]) { Scale = new Vector3(0.02f, 0.02f, 1f) });
            UI.Add(new TextObject("SHIELDS 9999", Program.FontSets["DebugFont"], Program.Shaders["texture_shader"]) { Scale = new Vector3(0.02f, 0.02f, 1f) });
        }

        public void Update(List<SpaceObject> objs, KeyboardState keybd, GameCursor cursor, double gametime)
        {
            if (Target == null) { Target = this; }

            for (int i = 0; i < Projectiles.Count; i++)
            {
                var p = Projectiles[i];
                if (Distance(p.Position) > 50) { Projectiles.Remove(p); }
                else { p.Thrust(1, 1); }
            }

            SpaceObject inSOI = this;
            SpaceObject inRad = this;
            SpaceObject cursorTarget = this;
            SpaceObject clickTarget = this;
            SpaceObject closestTarget = this;
            float closestDist = float.MaxValue;
            foreach (var obj in objs)
            {
                if (obj.Collidable && Distance(obj.Position) < obj.SOI && obj != this) { inSOI = obj; }
                if (Distance(obj.Position) < Radius) { inRad = obj; }
                float dist = Mag(obj.Position.X - Position.X, obj.Position.Y - Position.Y);
                if (dist < closestDist && obj.ObjectState == SpaceObjectState.ALIVE && obj != this) { closestTarget = obj; closestDist = dist; }                
                if (Mag(obj.Position.X - cursor.X, obj.Position.Y - cursor.Y) < obj.SOI * 3 && obj.ObjectState == SpaceObjectState.ALIVE) { cursorTarget = obj; }
                if (cursor.LeftReleased && Mag(obj.Position.X - cursor.X, obj.Position.Y - cursor.Y) < obj.Radius) { clickTarget = obj; }

                float dmg = 0;
                float apen = 0;
                float spen = 0;
                for (int i = 0; i < Projectiles.Count; i++)
                {
                    var p = Projectiles[i];
                    if (obj.Distance(p.Position) < obj.Radius && obj.Team != Team && obj.Collidable)
                    {
                        dmg += p.Damage;
                        apen += p.ArmorPen;
                        spen += Mag(p.Velocity_X, p.Velocity_Y) * 10;
                        Projectiles.Remove(p);
                    }
                }
                foreach (var mod in obj.Modules)
                {
                    if (mod.Shields > 0)
                    {
                        dmg *= 10 / (10 + mod.Shields);
                        mod.Shields = Math.Max(mod.Shields - spen, 0);
                    }
                }

                foreach (var mod in obj.Modules)
                {
                    if (mod.Armor > 0)
                    {
                        dmg = Math.Max(dmg - mod.Armor, 0);
                        mod.Armor = Math.Max(mod.Armor - apen, 0);
                    }
                }

                obj.Health = Math.Max(obj.Health - dmg, 0);
            }

            switch (ObjectState)
            {
                case SpaceObjectState.ALIVE:
                    if (Health <= 0)
                    {
                        ObjectState = SpaceObjectState.DEAD;
                        Collidable = false;
                        SOI = 0;
                        foreach (var mod in Modules) { mod.Armed = false; mod.Enabled = false; }
                        foreach (var ui in UI) { ui.Enabled = false; }
                        DespawnSW.Start();
                    }
                    Health = Math.Min(Health + HealthRegen, HealthMax);
                    if (inSOI != this) { this.Flee(inSOI); }
                    else if (NPC)
                    {
                        if (Distance(Target.Position) < MinOrbit) { this.Orbit(Target); }
                        this.Approach(Target);
                        this.Point(Target);
                        foreach (var m in Modules) { m.Target = Target; }
                    }
                    else
                    {
                        this.WASDFly(keybd);
                        this.Point(cursor.X, cursor.Y);
                        foreach (var m in Modules)
                        {
                            if (cursorTarget != this) { m.Target = cursorTarget; }
                            else if (EnemyAI.Ships.Count > 0) { m.Target = closestTarget; }
                        }
                    }
                    break;

                case SpaceObjectState.DEAD:
                    if (DespawnSW.Elapsed.Seconds > 30) { ObjectState = SpaceObjectState.TBD; }
                    break;

                case SpaceObjectState.TBD:
                    objs.Remove(this);
                    //DeleteTex();
                    break;

                default: break;
            }

            Armor = 0;
            bool hasArmor = false;
            Shields = 0;
            bool hasShields = false;
            foreach (var mod in Modules)
            {
                if (mod.Armed && mod.Target.ObjectState == SpaceObjectState.ALIVE && Distance(mod.Target.Position) < mod.Range && mod.Ammo != null && mod.Target != this)
                {
                    mod.FireSW.Start();
                    if (mod.FireSW.ElapsedMilliseconds > mod.FireRate && Math.Abs(dTheta(mod.Target.Position)) < mod.FiringArc)
                    {
                        for (int i = 0; i < mod.Burst; i++)
                        {
                            Vector3 pos = Position;
                            float spread = mod.Spread / 2 * 3.14f / 180f / mod.Burst;
                            float dacc = (float)(WeaponAcc.NextDouble() - 0.5f) * mod.Accuracy;
                            float vx = Velocity_X;
                            float vy = Velocity_Y;
                            float dx = mod.Target.Position.X - Position.X;
                            float dy = mod.Target.Position.Y - Position.Y;
                            float ax = dx + mod.Target.Velocity_X;
                            float ay = dy + mod.Target.Velocity_Y;
                            float theta = (float)(Math.Atan(ay / ax));
                            if (dx < 0) { theta += 3.14f; }
                            theta += dacc * 3.14f / 180;
                            if (mod.Burst > 1) { theta += spread * (i - (mod.Burst - 1) / 2); }
                            ax = (float)Math.Cos(theta) * mod.Ammo.TopSpeed;
                            ay = (float)Math.Sin(theta) * mod.Ammo.TopSpeed;
                            
                            Projectiles.Add(new SpaceObject()
                            {
                                Enabled = true,
                                RenderSections = mod.Ammo.RenderSections,
                                Shader = mod.Ammo.Shader,
                                Scale = mod.Ammo.Scale,
                                Damage = mod.Ammo.Damage,
                                ArmorPen = mod.Ammo.ArmorPen,
                                Position = pos,
                                Rotation = new Vector3(0, 0, theta * 180f / 3.14f + 90),
                                Velocity_X = ax,
                                Velocity_Y = ay,
                                Acceleration_X = ax,
                                Acceleration_Y = ay,
                                TopSpeed = mod.Ammo.TopSpeed
                            });
                        }
                        mod.FireSW.Restart();
                    }
                }
                if (mod.ArmorMax > 0)
                {
                    hasArmor = true;
                    Armor += mod.Armor;
                    mod.Position = Position;
                    mod.RenderSections[0].Alpha = mod.Armor / mod.ArmorMax;
                }
                if (mod.ShieldMax > 0)
                {
                    hasShields = true;
                    mod.Shields = Math.Min(mod.Shields + mod.ShieldRegen, mod.ShieldMax);
                    Shields += mod.Shields;
                    mod.Position = Position;
                    mod.RenderSections[0].Alpha = mod.Shields / mod.ShieldMax;
                }
            }

            float uiPos = -2 * Scale.Y;
            UI[0].SetPosition(Position.X, Position.Y + uiPos, Position.Z);
            UI[0].Text = "HP " + Health.ToString("F0");
            UI[0].Enabled = ObjectState == SpaceObjectState.ALIVE;

            uiPos -= 0.5f;
            UI[1].SetPosition(Position.X, Position.Y + uiPos, Position.Z);
            UI[1].Text = "AR " + Armor.ToString("F2");
            UI[1].Enabled = hasArmor && ObjectState == SpaceObjectState.ALIVE;

            uiPos = hasArmor && ObjectState == SpaceObjectState.ALIVE ? uiPos - 0.5f : uiPos;
            UI[2].SetPosition(Position.X, Position.Y + uiPos, Position.Z);
            UI[2].Text = "SH " + Shields.ToString("F0");
            UI[2].Enabled = hasShields && ObjectState == SpaceObjectState.ALIVE;
        }
    }
}
