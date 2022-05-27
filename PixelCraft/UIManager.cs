using OpenTK;
using System;
using System.Collections.Generic;
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

        public static Dictionary<string, UIGroup> UIGroups = new Dictionary<string, UIGroup>()
        {
            { "DebugPanel", DebugPanel() },
            { "UpgradePanel", UpgradePanel() }
        };


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
                ((TextObject)go[3]).Text = "LGC=" + (gwin.avgLGC.Average() / 10000f / 60 * 100).ToString("F1") + "%  GUI=" + (gwin.render_sw.ElapsedTicks / 1f).ToString("F2");
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
                Position = new Vector3(0.8f, 0f, 1f),
                Scale = new Vector3(0.2f, 1f, 1f)
            });
            go.Add(new TextObject("SCORE 9999", Program.FontSets["UIFont"], Program.Shaders["debugText_shader"])
            {
                Position = new Vector3(0.65f, 0.9f, 1f),
                Scale = new Vector3(0.0015f, 0.0015f, 0f)
            });

            ui.UpdateDel = new Action<GameWindow>((gwin) =>
            {
                ((TextObject)go[1]).Text = "Score  " + AllyAI.PlayerShip.Score.ToString("F0");
            });

            return ui;
        }
    }
}
