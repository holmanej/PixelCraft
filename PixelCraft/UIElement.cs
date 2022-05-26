using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelCraft.RenderObject;

namespace PixelCraft
{
    public class UIElement
    {
        public List<RenderObject> Elements;
        public Shader Shader = Program.Shaders["debugText_shader"];

        public void Update(GameCursor cursor)
        {
            var player = AllyAI.PlayerShip;
            ((TextObject)Elements[1]).Text = "SCORE " + player.Score.ToString("F0");
            if (cursor.LeftReleased && player.Score >= 10)
            {
                player.Score -= 10;
                AllyAI.PlayerShip.Modules[2].Burst++;
            }
            if (cursor.RightReleased && player.Score >= 20)
            {
                player.Score -= 20;
                AllyAI.PlayerShip.Modules[1].Armed = true;
            }
        }

        public UIElement()
        {
            Elements = new List<RenderObject>();
            Elements.Add(new RenderObject()
            {
                RenderSections = new List<Section>() { Program.RenderSections["SidePanel"] },
                Shader = Shader,
                Position = new Vector3(0.8f, 0f, 1f),
                Scale = new Vector3(0.2f, 1f, 1f)
            });
            Elements.Add(new TextObject("SCORE 9999", Program.FontSets["UIFont"], Shader)
            {
                Position = new Vector3(0.65f, 0.9f, 1f),
                Scale = new Vector3(0.0015f, 0.0015f, 0f)
            });
        }
    }
}
