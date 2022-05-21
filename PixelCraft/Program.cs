﻿using OpenTK;
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

                SetDir(@"/resources/models");

                gWin.SpaceObjects.Add("ClickSpot", new SpaceObject() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.1f, 0.1f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                gWin.SpaceObjects.Add("Player", new SpaceObject()
                {
                    RenderSections = Img2Sect(Textures["ShipCore"]),
                    Shader = Shaders["texture_shader"],
                    Position = new Vector3(-35f, 0f, 0f),
                    Scale = new Vector3(0.6f, 0.6f, 1f),
                    NPC = false,
                    SOI = 3f,
                    Collidable = true,
                    NowState = SpaceObject.SpaceObjectState.FLYING,
                    Health = 100
                });
                gWin.SpaceObjects.Add("Asteroid", new SpaceObject() { RenderSections = Img2Sect(Textures["asteroid"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, -1f), Scale = new Vector3(25f, 25f, 1f), Rotation = new Vector3(0, 0, 0), Radius = 25f, SOI = 28, Mass = 10 });
                //gWin.SpaceObjects.Add("Player_Turret", new SpaceObject() { RenderSections = Img2Sect(Textures["Turret"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, 0f), Scale = new Vector3(0.5f, 0.5f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                //gWin.SpaceObjects.Add("Player_Fab", new SpaceObject() { RenderSections = Img2Sect(Textures["Fabricator"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 4f, -0.1f), Scale = new Vector3(0.5f, 0.5f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                //gWin.SpaceObjects.Add("Player_Exca", new SpaceObject() { RenderSections = Img2Sect(Textures["Excavator"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 6f, -0.1f), Scale = new Vector3(0.5f, 0.5f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                //gWin.SpaceObjects["Player_Turret"].Attach(gWin.SpaceObjects["Player"]);
                //gWin.SpaceObjects["Player_Fab"].Attach(gWin.SpaceObjects["Player"]);
                //gWin.SpaceObjects["Player_Exca"].Attach(gWin.SpaceObjects["Player"]);
                gWin.SpaceObjects["Player"].UI.Add(gWin.SpaceObjects["Player"].Health, new TextObject("HEALTH 9999", Fonts["times"], Shaders["debugText_shader"]) { Position = new Vector3(-0.5f, -0.92f, 0), Scale = new Vector3(0.8f, 0.06f, 1f), Color = Color.White, BGColor = Color.Black, Size = 24 });

                //gWin.SpaceObjects.Add("starfield", new SpaceObject() { RenderSections = Img2Sect(Textures["StarField"]), Shader = Shaders["texture_shader"], Position = new Vector3(0f, 0f, -1.0f), Scale = new Vector3(50f, 50f, 1f), Rotation = new Vector3(0, 0, 0), SOI = 1f });
                gWin.SpaceObjects.Add("enemy", new SpaceObject()
                {
                    RenderSections = Img2Sect(Textures["Enemy"]),
                    Shader = Shaders["texture_shader"],
                    Position = new Vector3(-40f, 0f, 0f),
                    Scale = new Vector3(0.6f, 0.6f, 1f),
                    SOI = 1f,
                    Collidable = true,
                    NowState = SpaceObject.SpaceObjectState.FLYING,
                    Target = gWin.SpaceObjects["Player"]
                });

                sw.Stop();
                Debug.WriteLine("Load Time: " + sw.Elapsed);
                gWin.VSync = VSyncMode.Off;
                gWin.Run(60, 0);
            }
        }

        static List<RenderObject.Section> Img2Sect(Image img)
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
