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

        static void Main(string[] args)
        {
            using (GameWindow gWin = new GameWindow(1200, 900, "OpenTK Template"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Shaders = LoadShaders();
                Fonts = LoadFonts();

                SetDir(@"/resources/models");

                var core_image = new GLTFObject(new GLTF_Converter("playerCore_v1.gltf"), Shaders["texture_shader"]);
                var turret_image = new GLTFObject(new GLTF_Converter("turret_v0.gltf"), Shaders["texture_shader"]);
                var fab_image = new GLTFObject(new GLTF_Converter("fabricator_v0.gltf"), Shaders["texture_shader"]);
                var exca_iamge = new GLTFObject(new GLTF_Converter("excavator_v0.gltf"), Shaders["texture_shader"]);
                var asteroid_image = new GLTFObject(new GLTF_Converter("asteroid_v2.gltf"), Shaders["texture_shader"]);

                gWin.GameObjects.Add("Player_Core", new ShipCore() { RenderSections = core_image.RenderSections, Shader = core_image.Shader, Position = new Vector3(-35f, 0f, 0f), Scale = new Vector3(0.15f, 0.1f, 0.15f), Rotation = new Vector3(90, 0, 0), SOI = 2.5f, Collidable = true });
                gWin.GameObjects.Add("Asteroid", new Asteroid() { RenderSections = asteroid_image.RenderSections, Shader = asteroid_image.Shader, Position = new Vector3(0f, 0f, -1f), Scale = new Vector3(29f, 0.1f, 29f), Rotation = new Vector3(90, 0, 0), SOI = 29 });
                gWin.GameObjects.Add("Player_Turret", new SpaceObject() { RenderSections = turret_image.RenderSections, Shader = turret_image.Shader, Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.15f, 0.1f, 0.15f), Rotation = new Vector3(90, 0, 270), SOI = 0.5f });
                gWin.GameObjects.Add("Player_Fab", new SpaceObject() { RenderSections = fab_image.RenderSections, Shader = fab_image.Shader, Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.15f, 0.1f, 0.15f), Rotation = new Vector3(90, 0, 0), SOI = 0.5f });
                gWin.GameObjects.Add("Player_Exca", new SpaceObject() { RenderSections = exca_iamge.RenderSections, Shader = exca_iamge.Shader, Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.15f, 0.1f, 0.15f), Rotation = new Vector3(90, 0, 180), SOI = 0.5f });

                ((ShipCore)gWin.GameObjects["Player_Core"]).AddOrbiter((SpaceObject)gWin.GameObjects["Player_Turret"]);
                ((ShipCore)gWin.GameObjects["Player_Core"]).AddOrbiter((SpaceObject)gWin.GameObjects["Player_Fab"]);
                ((ShipCore)gWin.GameObjects["Player_Core"]).AddOrbiter((SpaceObject)gWin.GameObjects["Player_Exca"]);

                //Random rand = new Random();
                //for (int i = 0; i < 100; i++)
                //{
                //    int x = rand.Next(-50, 50);
                //    int y = rand.Next(-50, 50);
                //    float s = (float)rand.NextDouble() / 10;
                //    gWin.GameObjects.Add("Asteroid" + i.ToString(), new GLTFObject(new GLTF_Converter("asteroid_v1.gltf"), Shaders["texture_shader"]) { Position = new Vector3(x, y, -11f), Scale = new Vector3(s, 0.01f, s), Rotation = new Vector3(90, 0, 0), Collidable = true });
                //}

                sw.Stop();
                Debug.WriteLine("Load Time: " + sw.Elapsed);
                gWin.VSync = VSyncMode.Off;
                gWin.Run(60, 0);
            }
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

        //static Dictionary<string, Texture> LoadTextures()
        //{
        //    SetDir(@"/resources/textures");

        //    Debug.WriteLine("Loading Textures");
        //    Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        //    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
        //    foreach (string f in files)
        //    {
        //        Debug.WriteLine(f);
        //        Texture texture = new Texture(f);
        //        string label = f.Substring(f.LastIndexOf('\\') + 1).Split('.')[0];
        //        Debug.WriteLine(label);

        //        textures.Add(label, texture);
        //    }

        //    return textures;
        //}

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
