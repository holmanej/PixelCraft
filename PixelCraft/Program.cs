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
            using (GameWindow gWin = new GameWindow(1200, 900, "OpenTK Template"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Shaders = LoadShaders();
                Fonts = LoadFonts();
                Textures = LoadTextures();

                AllyAI.Shaders = Shaders;
                AllyAI.Fonts = Fonts;
                AllyAI.Textures = Textures;
                EnemyAI.Shaders = Shaders;
                EnemyAI.Fonts = Fonts;
                EnemyAI.Textures = Textures;

                SetDir(@"/resources/models");

                gWin.SpaceObjects.Add(new SpaceObject() { RenderSections = Img2Sect(Textures["StarField"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, 0.2f), Scale = new Vector3(50f, 50f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f, Collidable = false });
                gWin.CursorImage = new SpaceObject() { RenderSections = Img2Sect(Textures["Cursor"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.4f, 0.4f, 1f), Rotation = new Vector3(0, 0, 45f), SOI = 1f, Collidable = false };
                gWin.SpaceObjects.Add(gWin.CursorImage);

                Random rand = new Random();
                List<SpaceObject> asteroids = new List<SpaceObject>();
                for (int i = 0; i < 15; i++)
                {
                    int x = rand.Next(-50, 50);
                    int y = rand.Next(-50, 50);
                    int size = rand.Next(1, 6) / 2;
                    //foreach (var ast in asteroids)
                    //{
                    //    while (ast.Distance(new Vector3(x, y, 0.1f)) > ast.SOI)
                    //    {
                    //        x = rand.Next(-50, 50);
                    //        y = rand.Next(-50, 50);
                    //    }
                    //}
                    asteroids.Add(new SpaceObject() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"], Position = new Vector3(x, y, 0.1f), Scale = new Vector3(size, size, 1f), Rotation = new Vector3(0, 0, 0), Radius = size, SOI = size * 1.2f, Team = 0 });
                }
                gWin.SpaceObjects.AddRange(asteroids);
                gWin.PlayerObject = AllyAI.BuildCore();
                gWin.SpaceObjects.Add(gWin.PlayerObject);

                gWin.SpaceObjects = gWin.SpaceObjects.OrderBy(o => o.Position.Z).ToList();

                sw.Stop();
                Debug.WriteLine("Load Time: " + sw.Elapsed);
                gWin.VSync = VSyncMode.Off;
                gWin.Run(60, 0);
            }
        }

        public static List<RenderObject.Section> Img2Sect(Image img)
        {
            return new List<RenderObject.Section>() { new RenderObject.Section((Bitmap)img) };
        }

        public static List<RenderObject.Section> Img2Sect(Image alive, Image dead)
        {
            var sects = new List<RenderObject.Section>();
            sects.Add(new RenderObject.Section((Bitmap)alive));
            sects.Add(new RenderObject.Section((Bitmap)dead) { Visible = false });
            return sects;
        }

        public static List<RenderObject.Section> Img2Sect(List<Image> imgs)
        {
            var sects = new List<RenderObject.Section>();
            foreach (var i in imgs) { sects.Add(new RenderObject.Section((Bitmap)i)); }
            return sects;
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
