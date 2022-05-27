using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
        static public Dictionary<string, RenderObject> FontSets;
        static public Dictionary<string, Image> Textures;
        static public Dictionary<string, RenderObject.Section> RenderSections;

        static void Main(string[] args)
        {
            using (GameWindow gWin = new GameWindow(1200, 900, "OpenTK Template"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                Shaders = LoadShaders();
                Fonts = LoadFonts();
                Textures = LoadTextures();

                FontSets = new Dictionary<string, RenderObject>();
                FontSets.Add("DebugFont", CreateFontsetRender(Fonts["times"], Color.White, Color.Black, 24, Shaders["debugText_shader"]));
                FontSets.Add("UIFont", CreateFontsetRender(Fonts["times"], Color.Black, Color.White, 24, Shaders["debugText_shader"]));

                AllyAI.Shaders = Shaders;
                AllyAI.Fonts = Fonts;
                AllyAI.RenderSections = RenderSections;
                AllyAI.Update(gWin.SpaceObjects);
                EnemyAI.Shaders = Shaders;
                EnemyAI.Fonts = Fonts;
                EnemyAI.RenderSections = RenderSections;
                EnemyAI.Update(gWin.SpaceObjects, AllyAI.PlayerShip);

                SetDir(@"/resources/models");

                gWin.SpaceObjects.Add(new SpaceObject() { RenderSections = Img2Sect(Textures["StarField"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, 0.2f), Scale = new Vector3(50f, 50f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f, Collidable = false, ObjectState = SpaceObject.SpaceObjectState.INERT });

                Random rand = new Random();
                List<SpaceObject> asteroids = new List<SpaceObject>();
                for (int i = 0; i < 15; i++)
                {
                    int x = rand.Next(-50, 50);
                    int y = rand.Next(-50, 50);
                    int size = rand.Next(1, 6) / 2;
                    asteroids.Add(new SpaceObject() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"], Position = new Vector3(x, y, 0.1f), Scale = new Vector3(size, size, 1f), Rotation = new Vector3(0, 0, 0), Radius = size, SOI = size * 1.5f, Team = 0, Collidable = true, ObjectState = SpaceObject.SpaceObjectState.INERT });
                }
                gWin.SpaceObjects.AddRange(asteroids);

                //AllyAI.PlayerShip = AllyAI.BuildCore();
                //gWin.SpaceObjects.Add(AllyAI.PlayerShip);

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

        private static RenderObject CreateFontsetRender(FontFamily fontfamily, Color fgColor, Color bgColor, int size, Shader shader)
        {
            var fontset = new RenderObject() { Shader = shader };
            fontset.RenderSections = new List<RenderObject.Section>();
            Font font = new Font(fontfamily, size, GraphicsUnit.Pixel);
            for (int i = 32; i < 127; i++)
            {
                Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);
                SizeF charSize = g.MeasureString(Convert.ToChar(i).ToString(), font);
                Bitmap charBmp = new Bitmap((int)charSize.Width, font.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                g = Graphics.FromImage(charBmp);
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.Clear(bgColor);
                g.DrawString(Convert.ToChar(i).ToString(), font, new SolidBrush(fgColor), 0, 0, StringFormat.GenericTypographic);
                fontset.RenderSections.Add(new RenderObject.Section(charBmp) { ImageHandle = GL.GenTexture() });
                bmp.Dispose();
                charBmp.Dispose();
            }

            return fontset;
        }

        static Dictionary<string, Image> LoadTextures()
        {
            SetDir(@"/resources/textures");

            Debug.WriteLine("Loading Textures");
            Dictionary<string, Image> textures = new Dictionary<string, Image>();
            RenderSections = new Dictionary<string, RenderObject.Section>();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
            foreach (string f in files)
            {
                //Debug.WriteLine(f);
                Image texture = Image.FromFile(f);
                string label = f.Substring(f.LastIndexOf('\\') + 1).Split('.')[0];
                Debug.WriteLine(label);
                textures.Add(label, texture);
                RenderSections.Add(label, new RenderObject.Section((Bitmap)texture));
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
