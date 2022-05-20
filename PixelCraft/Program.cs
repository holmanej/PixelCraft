using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    static class Program
    {
        static public Dictionary<string, Shader> Shaders;
        static public Dictionary<string, FontFamily> Fonts;
        static public Dictionary<string, Image> Textures;

        static void Main(string[] args)
        {
            using (GameWindow gWin = new GameWindow(1000, 750, "OpenTK Template"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Shaders = LoadShaders();
                Fonts = LoadFonts();
                Textures = LoadTextures();

                SetDir(@"/resources/models");

                var bullet = new SpaceObject() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"] };
                gWin.GameObjects.Add("Asteroid", new Asteroid() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, -0.2f), Scale = new Vector3(25f, 25f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 25f });
                gWin.GameObjects.Add("Player_Turret", new SpaceObject() { RenderSections = Img2Sect(Textures["Turret"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, -0.1f), Scale = new Vector3(0.5f, 0.5f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                gWin.GameObjects.Add("Player_Fab", new SpaceObject() { RenderSections = Img2Sect(Textures["Fabricator"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, -0.1f), Scale = new Vector3(0.5f, 0.5f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                gWin.GameObjects.Add("Player_Exca", new SpaceObject() { RenderSections = Img2Sect(Textures["Excavator"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, -0.1f), Scale = new Vector3(0.5f, 0.5f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                gWin.GameObjects.Add("Player_Core", new ShipCore() { RenderSections = Img2Sect(Textures["ShipCore"]), Shader = Shaders["texture_shader"], Position = new Vector3(-35f, 0f, -0.1f), Scale = new Vector3(0.6f, 0.6f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f, Collidable = true });
                //gWin.GameObjects.Add("starfield", new SpaceObject() { RenderSections = Img2Sect(Textures["StarField"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, -0.3f), Scale = new Vector3(50f, 50f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f, Collidable = true });
                gWin.GameObjects.Add("enemy", new Enemy() { RenderSections = Img2Sect(Textures["Enemy"]), Shader = Shaders["texture_shader"], Position = new Vector3(-40f, 0f, 0f), Scale = new Vector3(0.6f, 0.6f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f, Collidable = true });
                gWin.GameObjects.Add("ClickSpot", new Asteroid() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.1f, 0.1f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });

                ((ShipCore)gWin.GameObjects["Player_Core"]).AddOrbiter((SpaceObject)gWin.GameObjects["Player_Turret"]);
                ((ShipCore)gWin.GameObjects["Player_Core"]).AddOrbiter((SpaceObject)gWin.GameObjects["Player_Fab"]);
                ((ShipCore)gWin.GameObjects["Player_Core"]).AddOrbiter((SpaceObject)gWin.GameObjects["Player_Exca"]);

                //Random rand = new Random();
                //for (int i = 0; i < 100; i++)
                //{
                //    int x = rand.Next(-50, 50);
                //    int y = rand.Next(-50, 50);
                //    float s = (float)rand.NextDouble() / 10;
                //    gWin.GameObjects.Add("Asteroid" + i.ToString(), new Asteroid() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"], Position = new Vector3(x, y, -0.1f), Scale = new Vector3(s, s, 1f), Rotation = new Vector3(0, 0, 0), Collidable = true });
                //}

                sw.Stop();
                Debug.WriteLine("Load Time: " + sw.Elapsed);
                gWin.VSync = VSyncMode.Off;
                gWin.Run(60, 0);
            }
        }

        static List<GameObject.Section> Img2Sect(Image img)
        {
            return new List<GameObject.Section>() { new GameObject.Section((Bitmap)img) };
        }

        static Dictionary<string, Shader> LoadShaders()
        {
            SetDir(@"/resources/shaders");

            Debug.WriteLine("Loading Shaders");
            Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());

            for (int i = 0; i < files.Length; i += 2)
            {
                //Debug.WriteLine(files[i + 1]);
                Shader shader = new Shader(files[i + 1], files[i]);
                string label = files[i].Substring(files[i].LastIndexOf('\\') + 1).Split('.')[0];
                Debug.WriteLine(label);

                shaders.Add(label, shader);
            }

            return shaders;
        }

        static Dictionary<string, FontFamily> LoadFonts()
        {
            SetDir(@"/resources/fonts");
            Debug.WriteLine("Loading Fonts");

            Dictionary<string, FontFamily> fonts = new Dictionary<string, FontFamily>();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.ttf");
            foreach (string f in files)
            {
                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile(f);
                string label = f.Substring(f.LastIndexOf('\\') + 1).Split('.')[0];
                Debug.WriteLine(label);

                fonts.Add(label, pfc.Families[0]);
            }

            return fonts;
        }

        static Dictionary<string, Image> LoadTextures()
        {
            SetDir(@"/resources/textures");

            Debug.WriteLine("Loading Textures");
            Dictionary<string, Image> textures = new Dictionary<string, Image>();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
            foreach (string f in files)
            {
                //Debug.WriteLine(f);
                Image texture = Image.FromFile(f);
                string label = f.Substring(f.LastIndexOf('\\') + 1).Split('.')[0];
                Debug.WriteLine(label);

                textures.Add(label, texture);
            }

            return textures;
        }

        public static void SetDir(string name)
        {
            for (int i = 0; i < 10 && !Directory.GetCurrentDirectory().EndsWith("PixelCraft"); i++)
            {
                Directory.SetCurrentDirectory("..");
            }
            Directory.SetCurrentDirectory("." + name);
            //Debug.WriteLine("Setting Directory");
            //Debug.WriteLine(Directory.GetCurrentDirectory());
        }
    }
}
