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
            public Action UpdateDel;
            public bool Enabled = false;
        }

        public class SmallPanelButton
        {
            public List<RenderObject> Gobjs = new List<RenderObject>();
            public int Cost;

            public SmallPanelButton(Section image, string desc, int cost, float x, float y)
            {
                Cost = cost;
                float dx = 0.1f * x + 0.65f;
                float dy = 0.15f * y + 0.8f;
                Gobjs.Add(new RenderObject()
                {
                    RenderSections = new List<Section>() { image },
                    Shader = Program.Shaders["debugText_shader"],
                    Position = new Vector3(dx, dy, 0f),
                    Scale = new Vector3(0.04f, 0.05f, 1f),
                });
                Gobjs.Add(new TextObject("", Program.FontSets["UIFont"], Program.Shaders["debugText_shader"])
                {
                    Position = new Vector3(dx - 0.03f, dy - 0.055f, 0f),
                    Scale = new Vector3(0.001f, 0.001f, 0f)
                });
                Gobjs.Add(new TextObject(cost.ToString("F0"), Program.FontSets["UIFont"], Program.Shaders["debugText_shader"])
                {
                    Position = new Vector3(dx - 0.01f, dy + 0.045f, 0f),
                    Scale = new Vector3(0.0015f, 0.0015f, 0f)
                });
                Gobjs.Add(new TextObject(desc, Program.FontSets["HUDFont"], Program.Shaders["debugText_shader"])
                {
                    Position = new Vector3(dx - 0.03f, dy - 0.085f, 0f),
                    Scale = new Vector3(0.001f, 0.0014f, 0f)
                });
            }

            public int Upgrade(Action del, string text, bool cond)
            {
                ((TextObject)Gobjs[1]).Text = text;
                if (Gobjs[0].LClicked() && AllyAI.PlayerShip.Score >= Cost && cond)
                {
                    del();
                    return Cost;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static Dictionary<string, UIGroup> UIGroups = new Dictionary<string, UIGroup>()
        {
            { "DebugPanel", DebugPanel() },
            { "UpgradePanel", UpgradePanel() },
            { "Title", TitleScreen() },
            { "DeathScreen", DeathScreen() },
            { "WinScreen", WinScreen() },
            { "Countdown", Countdown() }
        };

        public static GameWindow Gwin;
        public static GameCursor Cursor;

        public static UIGroup DebugPanel()
        {
            var ui = new UIGroup();
            var go = ui.GraphicsObjects;

            go.Add(new TextObject("POS XYZ", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.92f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            go.Add(new TextObject("GAMETIME X.X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.84f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            go.Add(new TextObject("FPS X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.76f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            go.Add(new TextObject("SW X", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.99f, 0.68f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            ui.UpdateDel = new Action(() =>
            {
                ((TextObject)go[0]).Text = "X=" + Gwin.ViewX.ToString("F2") + "  Y=" + Gwin.ViewY.ToString("F2") + "  Z=" + Gwin.ViewZ.ToString("F3");
                ((TextObject)go[1]).Text = "Gametime=" + Gwin.GameTime.ToString("F1");
                ((TextObject)go[2]).Text = "FPS=" + (int)Gwin.avgFPS.Average();
                ((TextObject)go[3]).Text = "LGC=" + (Gwin.avgLGC.Average() / 10000f / 60 * 100).ToString("F1") + "%  BLT=" + (Gwin.bulletCnt / 1000f).ToString("F1") + "k";
                //((TextObject)go[3]).Text = "LGC=" + (gwin.avgLGC.Average() / 10000f / 60 * 100).ToString("F1") + "%  GUI=" + (gwin.render_sw.ElapsedTicks / 1f).ToString("F2");
            });

            return ui;
        }

        public static UIGroup TitleScreen()
        {
            var ui = new UIGroup();
            var go = ui.GraphicsObjects;

            go.Add(new TextObject("PixelCraft", Program.FontSets["BigFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.7f, 0.5f, 0), Scale = new Vector3(0.003f, 0.003f, 1f) });
            go.Add(new TextObject("Gun Test", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.5f, 0f, 0), Scale = new Vector3(0.003f, 0.003f, 1f) });
            go.Add(new TextObject("Arcade", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.5f, -0.1f, 0), Scale = new Vector3(0.003f, 0.003f, 1f) });
            go.Add(new RenderObject()
            {
                RenderSections = new List<Section>() { Program.RenderSections["WhiteBorder"] },
                Shader = Program.Shaders["debugText_shader"],
                Position = new Vector3(-0.3f, 0.05f, 0f),
                Scale = new Vector3(0.3f, 0.05f, 1f),
            });
            go.Add(new RenderObject()
            {
                RenderSections = new List<Section>() { Program.RenderSections["WhiteBorder"] },
                Shader = Program.Shaders["debugText_shader"],
                Position = new Vector3(-0.3f, -0.05f, 0f),
                Scale = new Vector3(0.3f, 0.05f, 1f),
            });

            ui.UpdateDel = new Action(() =>
            {
                if (go[3].LClicked()) { WorldManager.ChangeLevel("GunTest"); Debug.WriteLine("pew"); }
                if (go[4].LClicked()) { WorldManager.ChangeLevel("Arcade"); Debug.WriteLine("ding ding"); }
            });

            ui.Enabled = true;
            return ui;
        }

        public static UIGroup DeathScreen()
        {
            var ui = new UIGroup();
            var go = ui.GraphicsObjects;

            go.Add(new TextObject("HA U SUCK", Program.FontSets["DedFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.7f, 0f, 0), Scale = new Vector3(0.003f, 0.003f, 1f) });
            ui.UpdateDel = new Action(() =>
            {
                go[0].AdjustAlpha(0.002f);
            });

            return ui;
        }

        public static UIGroup WinScreen()
        {
            var ui = new UIGroup();
            var go = ui.GraphicsObjects;

            go.Add(new TextObject("OK U WIN", Program.FontSets["WinFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.7f, 0f, 0), Scale = new Vector3(0.003f, 0.003f, 1f) });
            ui.UpdateDel = new Action(() =>
            {
                go[0].AdjustAlpha(0.002f);
            });

            return ui;
        }

        public static UIGroup Countdown()
        {
            var ui = new UIGroup();
            var go = ui.GraphicsObjects;

            go.Add(new TextObject("PREPARE", Program.FontSets["BigFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.5f, -0.7f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });
            go.Add(new TextObject("", Program.FontSets["BigFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.5f, -0.8f, 0), Scale = new Vector3(0.002f, 0.002f, 1f) });

            ui.UpdateDel = new Action(() =>
            {
                TimeSpan t = WorldManager.SpawnTimer.Elapsed - TimeSpan.FromSeconds(20);
                if (WorldManager.SpawnTimer.ElapsedMilliseconds < 20000)
                {
                    go[0].Enabled = true;
                    ((TextObject)go[1]).Text = (t.TotalMilliseconds / 1000f).ToString("F3");
                }
                else
                {
                    go[0].Enabled = false;
                    ((TextObject)go[1]).Text = t.Minutes + ":" + ((t.TotalMilliseconds % 60000) / 1000f).ToString("F3").PadLeft(6, '0');
                }
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
            // SHIP UPGRADES
            var HealthRegen = new SmallPanelButton(Program.RenderSections["HRegen_Up"], "HR+", 2, 0, 0); go.AddRange(HealthRegen.Gobjs);
            var ArmorUp = new SmallPanelButton(Program.RenderSections["Armor_Up"], "AR+", 8, 1, 0); go.AddRange(ArmorUp.Gobjs);
            var ShieldMax = new SmallPanelButton(Program.RenderSections["SMax_Up"], "SM+", 5, 2, 0); go.AddRange(ShieldMax.Gobjs);
            var ShieldRegen = new SmallPanelButton(Program.RenderSections["SRegen_Up"], "SR+", 4, 3, 0); go.AddRange(ShieldRegen.Gobjs);

            var GR = new SmallPanelButton(Program.RenderSections["GRGun"], "BUY", 15, 0, -2); go.AddRange(GR.Gobjs);
            var GRD = new SmallPanelButton(Program.RenderSections["Dmg_Up"], "DMG", 10, 1, -2); go.AddRange(GRD.Gobjs);
            var GRF = new SmallPanelButton(Program.RenderSections["FRate_Up"], "RATE", 10, 2, -2); go.AddRange(GRF.Gobjs);
            var GRA = new SmallPanelButton(Program.RenderSections["Acc_Up"], "ACC", 5, 3, -2); go.AddRange(GRA.Gobjs);

            var FB = new SmallPanelButton(Program.RenderSections["FBGun"], "BUY", 20, 0, -4); go.AddRange(FB.Gobjs);
            var FBD = new SmallPanelButton(Program.RenderSections["Dmg_Up"], "DMG", 10, 1, -4); go.AddRange(FBD.Gobjs);
            var FBF = new SmallPanelButton(Program.RenderSections["FRate_Up"], "RATE", 15, 2, -4); go.AddRange(FBF.Gobjs);
            var FBR = new SmallPanelButton(Program.RenderSections["Range_Up"], "RANGE", 10, 3, -4); go.AddRange(FBR.Gobjs);

            var SP = new SmallPanelButton(Program.RenderSections["SPGun"], "BUY", 25, 0, -6); go.AddRange(SP.Gobjs);
            var SPD = new SmallPanelButton(Program.RenderSections["Dmg_Up"], "DMG", 10, 1, -6); go.AddRange(SPD.Gobjs);
            var SPF = new SmallPanelButton(Program.RenderSections["FRate_Up"], "RATE", 20, 2, -6); go.AddRange(SPF.Gobjs);
            var SPA = new SmallPanelButton(Program.RenderSections["Acc_Up"], "ACC", 15, 3, -6); go.AddRange(SPA.Gobjs);

            var GB = new SmallPanelButton(Program.RenderSections["GBGun"], "BUY", 10, 0, -8); go.AddRange(GB.Gobjs);
            var GBD = new SmallPanelButton(Program.RenderSections["Dmg_Up"], "DMG", 10, 1, -8); go.AddRange(GBD.Gobjs);
            var GBF = new SmallPanelButton(Program.RenderSections["FRate_Up"], "RATE", 15, 2, -8); go.AddRange(GBF.Gobjs);
            var GBA = new SmallPanelButton(Program.RenderSections["Acc_Up"], "ACC", 5, 3, -8); go.AddRange(GBA.Gobjs);



            ui.UpdateDel = new Action(() =>
            {
                var ps = AllyAI.PlayerShip;
                go.OrderByDescending(o => o.Position.Z);
                ((TextObject)go[1]).Text = "Score  " + ps.Score.ToString("F0");
                ps.Score -= HealthRegen.Upgrade(new Action(() => { ps.HealthRegen += 0.0005f; }), (ps.HealthRegen * 60).ToString("F2"), ps.HealthRegen < 5);
                ps.Score -= ArmorUp.Upgrade(new Action(() => { ps.Modules[1].Armor++; }), ps.Modules[1].Armor.ToString("F2"), ps.Modules[1].Armor < 10);
                ps.Score -= ShieldMax.Upgrade(new Action(() => { ps.Modules[0].ShieldMax += 5f; }), ps.Modules[0].ShieldMax.ToString("F0"), ps.Modules[0].ShieldMax < 300);
                ps.Score -= ShieldRegen.Upgrade(new Action(() => { ps.Modules[0].ShieldRegen += 0.02f; }), (ps.Modules[0].ShieldRegen * 60).ToString("F1"), ps.Modules[0].ShieldRegen < 50);

                ps.Score -= GR.Upgrade(new Action(() => { ps.Modules[2].Armed = true; }), "GR", ps.Modules[2].Armed == false);
                ps.Score -= GRD.Upgrade(new Action(() => { ps.Modules[2].Ammo.Damage += 0.2f; }), ps.Modules[2].Ammo.Damage.ToString("F1"), ps.Modules[2].Ammo.Damage < 6);
                ps.Score -= GRF.Upgrade(new Action(() => { ps.Modules[2].FireRate *= 0.9f; }), ps.Modules[2].FireRate.ToString("F0"), ps.Modules[2].FireRate > 20);
                ps.Score -= GRA.Upgrade(new Action(() => { ps.Modules[2].Accuracy -= 3f; }), ps.Modules[2].Accuracy.ToString("F0"), ps.Modules[2].Accuracy > 5);

                ps.Score -= FB.Upgrade(new Action(() => { ps.Modules[3].Armed = true; }), "FB", ps.Modules[3].Armed == false);
                ps.Score -= FBD.Upgrade(new Action(() => { ps.Modules[3].Ammo.Damage += 0.2f; }), ps.Modules[3].Ammo.Damage.ToString("F1"), ps.Modules[3].Ammo.Damage < 4);
                ps.Score -= FBF.Upgrade(new Action(() => { ps.Modules[3].FireRate *= 0.9f; }), ps.Modules[3].FireRate.ToString("F0"), ps.Modules[3].FireRate > 50);
                ps.Score -= FBR.Upgrade(new Action(() => { ps.Modules[3].Range += 1f; }), ps.Modules[3].Range.ToString("F0"), ps.Modules[3].Range < 25);

                ps.Score -= SP.Upgrade(new Action(() => { ps.Modules[4].Armed = true; }), "SP", ps.Modules[4].Armed == false);
                ps.Score -= SPD.Upgrade(new Action(() => { ps.Modules[4].Ammo.Damage += 0.5f; }), ps.Modules[4].Ammo.Damage.ToString("F1"), ps.Modules[4].Ammo.Damage < 12);
                ps.Score -= SPF.Upgrade(new Action(() => { ps.Modules[4].FireRate *= 0.9f; }), ps.Modules[4].FireRate.ToString("F0"), ps.Modules[4].FireRate > 100);
                ps.Score -= SPA.Upgrade(new Action(() => { ps.Modules[4].Accuracy -= 3f; }), ps.Modules[4].Accuracy.ToString("F0"), ps.Modules[4].Accuracy > 0);

                ps.Score -= GB.Upgrade(new Action(() => { ps.Modules[5].Armed = true; }), "GB", ps.Modules[5].Armed == false);
                ps.Score -= GBD.Upgrade(new Action(() => { ps.Modules[5].Ammo.Damage += 0.5f; }), ps.Modules[5].Ammo.Damage.ToString("F1"), ps.Modules[5].Ammo.Damage <= 10);
                ps.Score -= GBF.Upgrade(new Action(() => { ps.Modules[5].FireRate *= 0.8f; }), ps.Modules[5].FireRate.ToString("F0"), ps.Modules[5].FireRate > 50);
                ps.Score -= GBA.Upgrade(new Action(() => { ps.Modules[5].Accuracy -= 2.5f; }), ps.Modules[5].Accuracy.ToString("F0"), ps.Modules[5].Accuracy > 10);
            });

            return ui;
        }

        static bool LClicked(this RenderObject obj)
        {
            return Cursor.LeftReleased && Cursor.Mx > obj.Position.X - obj.Scale.X && Cursor.Mx < obj.Position.X + obj.Scale.X && Cursor.My > obj.Position.Y - obj.Scale.Y && Cursor.My < obj.Position.Y + obj.Scale.Y;
        }
    }
}
