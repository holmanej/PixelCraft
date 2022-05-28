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
    public static class UIManager
    {
        public class UIGroup
        {
            public List<RenderObject> GraphicsObjects = new List<RenderObject>();
            public Delegate UpdateDel;
        }

        public class UpgradeBtn
        {
            public List<RenderObject> Gobjs = new List<RenderObject>();
            public int Cost;
            public bool Enabled = true;

            public UpgradeBtn(Section image, int cost, int x, int y)
            {
                Cost = cost;
                float dx = x * 0.2f;
                float dy = y * -0.25f;
                Gobjs.Add(new RenderObject()
                {
                    RenderSections = new List<Section>() { image },
                    Shader = Program.Shaders["debugText_shader"],
                    Position = new Vector3(0.7f + dx, dy, 0.2f),
                    Scale = new Vector3(0.06f, 0.07f, 1f),
                });
                Gobjs.Add(new TextObject("", Program.FontSets["UIFont"], Program.Shaders["debugText_shader"])
                {
                    Position = new Vector3(0.67f + dx, dy -0.07f, 0.1f),
                    Scale = new Vector3(0.001f, 0.001f, 0f)
                });
                Gobjs.Add(new TextObject(cost.ToString("F0"), Program.FontSets["UIFont"], Program.Shaders["debugText_shader"])
                {
                    Position = new Vector3(0.69f + dx, dy + 0.07f, 0.1f),
                    Scale = new Vector3(0.0015f, 0.0015f, 0f)
                });
            }

            public int Upgrade(Delegate del, string text)
            {
                if (Gobjs[0].LClicked() && AllyAI.PlayerShip.Score >= Cost && Enabled)
                {
                    del.DynamicInvoke();
                    return Cost;
                }
                else
                {
                    ((TextObject)Gobjs[1]).Text = text;
                    return 0;
                }
            }
        }

        public static Dictionary<string, UIGroup> UIGroups = new Dictionary<string, UIGroup>()
        {
            { "DebugPanel", DebugPanel() },
            { "UpgradePanel", UpgradePanel() }
        };

        public static GameCursor Cursor;

        public static UIGroup DebugPanel()
        {
            var ui = new UIGroup();
            var go = ui.GraphicsObjects;

            go.Add(new TextObject("POS XYZ", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.92f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            go.Add(new TextObject("GAMETIME X.X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.84f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            go.Add(new TextObject("FPS X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.76f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            go.Add(new TextObject("SW X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.68f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            ui.UpdateDel = new Action<GameWindow>((gwin) =>
            {
                ((TextObject)go[0]).Text = "X=" + gwin.ViewX.ToString("F2") + "  Y=" + gwin.ViewY.ToString("F2") + "  Z=" + gwin.ViewZ.ToString("F3");
                ((TextObject)go[1]).Text = "Gametime=" + gwin.GameTime.ToString("F1");
                ((TextObject)go[2]).Text = "FPS=" + (int)gwin.avgFPS.Average();
                ((TextObject)go[3]).Text = "LGC=" + (gwin.avgLGC.Average() / 10000f / 60 * 100).ToString("F1") + "%  BLT=" + (gwin.bulletCnt / 1000).ToString("F1");
                //((TextObject)go[3]).Text = "LGC=" + (gwin.avgLGC.Average() / 10000f / 60 * 100).ToString("F1") + "%  GUI=" + (gwin.render_sw.ElapsedTicks / 1f).ToString("F2");
            });

            return ui;
        }

        public static UIGroup UpgradePanel()
        {
            var ui = new UIGroup();
            var go = ui.GraphicsObjects;

            go.Add(new RenderObject()
            {
                RenderSections = new List<Section>() { Program.RenderSections["SidePanel"] },
                Shader = Program.Shaders["debugText_shader"],
                Position = new Vector3(0.8f, 0f, 0.1f),
                Scale = new Vector3(0.2f, 1f, 1f),
            });
            go.Add(new TextObject("SCORE 9999", Program.FontSets["UIFont"], Program.Shaders["debugText_shader"])
            {
                Position = new Vector3(0.65f, 0.9f, 0f),
                Scale = new Vector3(0.0015f, 0.0015f, 0f)
            });
            var HealthRegen = new UpgradeBtn(Program.RenderSections["HeartPlus"], 2, 0, 0);
            go.AddRange(HealthRegen.Gobjs);
            var ShieldUp = new UpgradeBtn(Program.RenderSections["HeartPlus"], 2, 1, 0);
            go.AddRange(ShieldUp.Gobjs);
            var SpreadUp = new UpgradeBtn(Program.RenderSections["HeartPlus"], 15, 0, 1);
            go.AddRange(SpreadUp.Gobjs);
            var DamageUp = new UpgradeBtn(Program.RenderSections["HeartPlus"], 10, 1, 1);
            go.AddRange(DamageUp.Gobjs);
            var CannonAdd = new UpgradeBtn(Program.RenderSections["HeartPlus"], 25, 0, 2);
            go.AddRange(CannonAdd.Gobjs);
            var CannonUp = new UpgradeBtn(Program.RenderSections["HeartPlus"], 5, 1, 2);
            go.AddRange(CannonUp.Gobjs);

            ui.UpdateDel = new Action<GameWindow>((gwin) =>
            {
                go.OrderByDescending(o => o.Position.Z);
                //if (go[0].LClicked()) { AllyAI.PlayerShip.Score--; }
                ((TextObject)go[1]).Text = "Score  " + AllyAI.PlayerShip.Score.ToString("F0");
                AllyAI.PlayerShip.Score -= HealthRegen.Upgrade(new Action(() => { AllyAI.PlayerShip.HealthRegen += 0.0005f; }), (AllyAI.PlayerShip.HealthRegen * 60).ToString("F2"));
                AllyAI.PlayerShip.Score -= ShieldUp.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[0].ShieldMax += 5f; }), AllyAI.PlayerShip.Modules[0].ShieldMax.ToString("F0"));
                AllyAI.PlayerShip.Score -= SpreadUp.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[2].Burst++; }), AllyAI.PlayerShip.Modules[2].Burst.ToString("F0"));
                AllyAI.PlayerShip.Score -= DamageUp.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[2].Ammo.Damage *= 1.1f; }), AllyAI.PlayerShip.Modules[2].Ammo.Damage.ToString("F1"));
                AllyAI.PlayerShip.Score -= CannonAdd.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1].Armed = true; }), AllyAI.PlayerShip.Modules[1].Armed.ToString());
                AllyAI.PlayerShip.Score -= CannonUp.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1].FireRate *= 0.95f; }), AllyAI.PlayerShip.Modules[1].FireRate.ToString("F0"));
            });

            return ui;
        }

        static bool LClicked(this RenderObject obj)
        {
            return Cursor.LeftReleased && Cursor.Mx > obj.Position.X - obj.Scale.X && Cursor.Mx < obj.Position.X + obj.Scale.X && Cursor.My > obj.Position.Y - obj.Scale.Y && Cursor.My < obj.Position.Y + obj.Scale.Y;
        }
    }
}
