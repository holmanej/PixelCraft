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

            public int Upgrade(Action del, string text)
            {
                if (Gobjs[0].LClicked() && AllyAI.PlayerShip.Score >= Cost && Enabled)
                {
                    del();
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
            { "UpgradePanel", UpgradePanel() },
            { "Title", TitleScreen() }
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

            go.Add(new TextObject("PixelCraft", Program.FontSets["DebugFont"], Program.Shaders["debugText_shader"]) { Position = new Vector3(-0.7f, 0.5f, 0), Scale = new Vector3(0.008f, 0.008f, 1f) });

            var Guntest = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, -5, 0);
            go.AddRange(Guntest.Gobjs);
            var Arcade = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, -3, 0);
            go.AddRange(Arcade.Gobjs);

            ui.UpdateDel = new Action(() =>
            {
                Guntest.Upgrade(new Action(() => { ui.Enabled = false; WorldManager.ChangeLevel("GunTest"); }), "LVL 0");
                Arcade.Upgrade(new Action(() => { ui.Enabled = false; WorldManager.ChangeLevel("Arcade"); }), "LVL 1");
            });

            ui.Enabled = true;
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
            var FB = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 0, -1);
            go.AddRange(FB.Gobjs);
            var SB = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 1, -1);
            go.AddRange(SB.Gobjs);
            var GB = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 0, 0);
            go.AddRange(GB.Gobjs);
            var FR = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 1, 0);
            go.AddRange(FR.Gobjs);
            var SR = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 0, 1);
            go.AddRange(SR.Gobjs);
            var GR = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 1, 1);
            go.AddRange(GR.Gobjs);
            var FP = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 0, 2);
            go.AddRange(FP.Gobjs);
            var SP = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 1, 2);
            go.AddRange(SP.Gobjs);
            var GP = new UpgradeBtn(Program.RenderSections["HeartPlus"], 0, 0, 3);
            go.AddRange(GP.Gobjs);

            ui.UpdateDel = new Action(() =>
            {
                go.OrderByDescending(o => o.Position.Z);
                ((TextObject)go[1]).Text = "Score  " + AllyAI.PlayerShip.Score.ToString("F0");
                FB.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.FBGun(AmmoTypes.SDAmmo()); }), "FB");
                SB.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.SBGun(AmmoTypes.SDAmmo()); }), "SB");
                GB.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.GBGun(AmmoTypes.SDAmmo()); }), "GB");
                FR.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.FRGun(AmmoTypes.SDAmmo()); }), "FR");
                SR.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.SRGun(AmmoTypes.SDAmmo()); }), "SR");
                GR.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.GRGun(AmmoTypes.SDAmmo()); }), "GR");
                FP.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.FPGun(AmmoTypes.SDAmmo()); }), "FP");
                SP.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.SPGun(AmmoTypes.SDAmmo()); }), "SP");
                GP.Upgrade(new Action(() => { AllyAI.PlayerShip.Modules[1] = GunTypes.GPGun(AmmoTypes.SDAmmo()); }), "GP");
            });

            return ui;
        }

        static bool LClicked(this RenderObject obj)
        {
            return Cursor.LeftReleased && Cursor.Mx > obj.Position.X - obj.Scale.X && Cursor.Mx < obj.Position.X + obj.Scale.X && Cursor.My > obj.Position.Y - obj.Scale.Y && Cursor.My < obj.Position.Y + obj.Scale.Y;
        }
    }
}
