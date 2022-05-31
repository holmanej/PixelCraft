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
        static readonly int Team = 2;
        public static List<SpaceObject> Ships = new List<SpaceObject>();
        public static Dictionary<string, int> Roster = new Dictionary<string, int>()
        {
            { "fighter", 0 },
            { "gunship", 0 }
        };

        public static void Update()
        {
            for (int i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                if (ship.ObjectState == SpaceObject.SpaceObjectState.DEAD)
                {
                    AllyAI.PlayerShip.Score += ship.ScoreValue;
                    Roster[ship.Type]--;
                    Ships.Remove(ship);
                }
                if (AllyAI.Ships.Exists(s => s.Target == ship))
                {
                    ship.Target = AllyAI.Ships.Find(s => s.Target == ship);
                }
                else if (AllyAI.Ships.Count > 0)
                {
                    ship.Target = AllyAI.Ships.Last();
                }
            }
        }

        public static SpaceObject BuildFighter(float x, float y)
        {
            var enemy = new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>(),
                Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(x, y, 0f),
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Type = "fighter",
                Health = 50,
                HealthMax = 50,
                HealthRegen = 0,
                TopSpeed = 0.2f,
                Agility = 3f,
                Acceleration_X = 0.01f,
                Acceleration_Y = 0.01f,
                Friction = 0.05f,
                SOI = 1f,
                ScoreValue = 1,
                MaxOrbit = 6,
                MinOrbit = 2,
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Team = Team
            };
            enemy.AliveSection = Program.RenderSections["Enemy"];
            enemy.DeadSection = Program.RenderSections["Dednemy"];
            enemy.Modules.Add(GunTypes.SBGun(AmmoTypes.SDAmmo()));

            Ships.Add(enemy);
            Roster["fighter"]++;
            return enemy;
        }

        public static SpaceObject BuildGunship(float x, float y)
        {
            var enemy = new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>(),
                Shader = Program.Shaders["texture_shader"],
                Position = new Vector3(x, y, 0f),
                Scale = new Vector3(2.6f, 2.6f, 1f),
                Type = "gunship",
                Health = 250,
                HealthMax = 250,
                TopSpeed = 0.1f,
                Acceleration_X = 0.01f,
                Acceleration_Y = 0.01f,
                Friction = 0.01f,
                Agility = 0.7f,
                SOI = 3f,
                ScoreValue = 10,
                MaxOrbit = 15,
                MinOrbit = 0,
                ObjectState = SpaceObject.SpaceObjectState.ALIVE,
                Team = Team
            };
            enemy.AliveSection = Program.RenderSections["Enemy"];
            enemy.DeadSection = Program.RenderSections["Dednemy"];
            enemy.Modules.Add(GunTypes.GBGun(AmmoTypes.MSAmmo()));
            enemy.Modules.Add(GunTypes.FPGun(AmmoTypes.MDAmmo()));
            enemy.Modules.Add(GunTypes.NovaGun(AmmoTypes.LDAmmo()));
            enemy.Modules.Add(new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { new RenderObject.Section(Program.RenderSections["Shield"], false) },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(4f, 4f, 1f),
                Collidable = false,
                ArmorMax = 4,
                Armor = 4
            });

            Ships.Add(enemy);
            Roster["gunship"]++;
            return enemy;
        }
    }
}
