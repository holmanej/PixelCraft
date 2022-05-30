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
        static readonly int Team = 1;
        public static List<SpaceObject> Ships = new List<SpaceObject>();
        public static Dictionary<string, int> Roster = new Dictionary<string, int>()
        {
            { "fighter", 0 },
            { "tank", 0 }
        };

        public static void Update()
        {
            for (int i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                if (ship.ObjectState == SpaceObject.SpaceObjectState.DEAD && ship != PlayerShip)
                {
                    Roster[ship.Type]--;
                    Ships.Remove(ship);
                }
                if (EnemyAI.Ships.Exists(s => s.Target == PlayerShip))
                {
                    ship.Target = EnemyAI.Ships.Find(s => s.Target == PlayerShip);
                }
                else if (EnemyAI.Ships.Count > 0)
                {
                    ship.Target = EnemyAI.Ships.First();
                }
            }
        }

        public static SpaceObject BuildCore()
        {
            var section = new List<RenderObject.Section>();
            var core = new SpaceObject()
            {
                RenderSections = section,
                Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(0f, 0f, 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Type = "player",
                Team = Team,
                NPC = false,
                TopSpeed = 0.2f,
                Acceleration_X = 0.005f,
                Acceleration_Y = 0.005f,
                Friction = 0.3f,
                Agility = 3,
                SOI = 1f,
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Health = 100,
                HealthMax = 100,
                HealthRegen = 0.005f,
            };
            section.Add(new RenderObject.Section(Program.RenderSections["ShipCore"], true));
            section.Add(new RenderObject.Section(Program.RenderSections["ShipCore_Dead"], false));
            core.Modules.Add(new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { new RenderObject.Section(Program.RenderSections["Shield"], true) },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(1.5f, 1.5f, 1f),
                Collidable = false,
                ShieldMax = 50,
                ShieldRegen = 0.1f,
                Shields = 100
            });
            core.Modules.Add(new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { new RenderObject.Section(Program.RenderSections["Armor"], true) },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(1.5f, 1.5f, 1f),
                Collidable = false,
                ArmorMax = 10,
                Armor = 1
            });
            core.Modules.Add(GunTypes.GRGun(AmmoTypes.SDAmmo(), false));
            core.Modules.Add(GunTypes.FBGun(AmmoTypes.MPAmmo(), false));
            core.Modules.Add(GunTypes.SPGun(AmmoTypes.LSAmmo(), false));
            core.Modules.Add(GunTypes.GBGun(AmmoTypes.SDAmmo(), false));
            Ships.Add(core);

            return core;
        }

        public static SpaceObject BuildFighter(float x, float y)
        {
            var section = new List<RenderObject.Section>();
            var ally = new SpaceObject()
            {
                RenderSections = section,
                Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(x, y, 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Type = "fighter",
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
            section.Add(new RenderObject.Section(Program.RenderSections["Ally"], true));
            section.Add(new RenderObject.Section(Program.RenderSections["Ally_Dead"], false));
            ally.Modules.Add(GunTypes.GRGun(AmmoTypes.MSAmmo()));
            
            Ships.Add(ally);
            Roster["fighter"]++;
            return ally;
        }

        public static SpaceObject BuildTank(float x, float y)
        {
            var section = new List<RenderObject.Section>();
            var ally = new SpaceObject()
            {
                RenderSections = section,
                Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(x, y, 0f),
                Scale = new Vector3(3f, 1f, 1f),
                Type = "tank",
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
            section.Add(new RenderObject.Section(Program.RenderSections["Ally"], true));
            section.Add(new RenderObject.Section(Program.RenderSections["Ally_Dead"], false));
            ally.Modules.Add(GunTypes.FBGun(AmmoTypes.SDAmmo()));
            ally.Modules.Add(new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { new RenderObject.Section(Program.RenderSections["Shield"], true) },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(4f, 4f, 1f),
                Collidable = false,
                ShieldMax = 200, ShieldRegen = 0.05f, Shields = 200
            });

            Ships.Add(ally);
            Roster["tank"]++;
            return ally;
        }
    }
}
