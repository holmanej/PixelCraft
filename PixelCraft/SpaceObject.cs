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

        public float Armor = 1;
        public float ArmorRegen = 0;
        public float ArmorMax = 1;

        public float Shields = 1;
        public float ShieldRegen = 0.01f;
        public float ShieldMax = 1;

        public float Mass = 0.01f;
        public float CargoMax = 0.5f;

        // BEHAVIOR
        public bool NPC = true;
        public bool Selected = false;
        public bool Collidable = true;
        public float Aggression = 1;
        public float Morale = 1;
        public float Radius = 1;
        public float SOI = 2;

        // MOVEMENT
        public float Velocity_X;
        public float Velocity_Y;
        public float Acceleration = 0.005f;
        public float Friction = 0.3f;
        public float TopSpeed = 0.2f;
        public float OrbitRange = 2;
        public float Agility = 1;

        // WEAPON
        public float FireRate = 1;
        public float Burst = 1;
        public float Accuracy = 1;

        // PROJECTILE
        public float Damage = 0;
        public float ArmorPen = 0;

        // FABRICATION
        public float BuildRate = 1;

        // MINING
        public float MiningRate = 0;

        // STATE
        public enum SpaceObjectState { FLYING, LANDED, ANCHORED, AIRBORNE, DEAD };
        public SpaceObjectState NowState = SpaceObjectState.DEAD;
        public SpaceObjectState MvmtState = SpaceObjectState.FLYING;

        // RELATIONSHIPS
        public SpaceObject Target;
        public SpaceObject Host;
        public List<SpaceObject> Modules = new List<SpaceObject>();
        public List<SpaceObject> Materials = new List<SpaceObject>();
        public List<SpaceObject> Attached = new List<SpaceObject>();
        public List<SpaceObject> Ammo = new List<SpaceObject>();
        public Dictionary<object, TextObject> UI = new Dictionary<object, TextObject>();

        public void Detach()
        {
            NowState = MvmtState;
        }

        public void Attach(SpaceObject host)
        {
            Host = host;
            MvmtState = NowState;
            NowState = SpaceObjectState.ANCHORED;
        }

        public override void Update(List<SpaceObject> objs, KeyboardState keybd, GameCursor cursor, double gametime)
        {
            SpaceObject InSOI = this;
            SpaceObject InRad = this;
            foreach (var obj in objs)
            {
                if (obj.Collidable && obj.Mass > InSOI.Mass * 10 && Distance(obj.Position) < obj.SOI)
                {
                    InSOI = obj;
                }
                if (Distance(obj.Position) < obj.Radius)
                {
                    InRad = obj;

                    if (obj.Damage > 0)
                    {
                        float dmg = obj.Damage;
                        dmg *= 100 / (100 + Shields);
                        Shields = Math.Max(Shields - Mag(obj.Velocity_X, obj.Velocity_Y), 0);

                        dmg -= Armor;
                        Armor -= obj.ArmorPen;

                        Health -= dmg;
                    }
                }
            }

            if (Health <= 0) { NowState = SpaceObjectState.DEAD; }

            switch (NowState)
            {
                case SpaceObjectState.FLYING:
                    MvmtState = SpaceObjectState.FLYING;
                    if (InSOI != this)
                    {
                        this.Flee(InSOI); 
                    }
                    else if (NPC)
                    { 
                        this.Approach(Target);
                        this.Point(Target);
                    }
                    else
                    { 
                        this.WASDFly(keybd); 
                    }
                    break;

                case SpaceObjectState.LANDED:
                    MvmtState = SpaceObjectState.LANDED;
                    // ObjMvmt.Walk(NPC, Target, insoi)
                    break;

                case SpaceObjectState.AIRBORNE:
                    // ObjMvmt.Fall(Host, insoi)
                    break;

                case SpaceObjectState.ANCHORED:
                    if (Host.NowState == SpaceObjectState.LANDED)
                    {
                        
                    }
                    else if (Host.NowState == SpaceObjectState.FLYING)
                    {
                        this.Approach(Host);
                    }
                    break;

                case SpaceObjectState.DEAD:
                    break;

                default:
                    break;
            }

            foreach (var obj in UI)
            {
                obj.Value.Text = obj.Key.ToString();
            }
        }
    }
}
