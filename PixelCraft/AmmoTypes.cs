using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{

    /* TYPE DMG PEN SPD
     * SD   3   0   3
     * MD   5   1   4
     * LD   10  2   5
     * 
     * SP   0   5   5
     * MP   2   8   6
     * LP   4   10  8
     * 
     * SS   1   1   8
     * MS   2   2   10
     * LS   3   3   12
    */
    public static class AmmoTypes
    {
        public static SpaceObject SDAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["WhiteBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.4f, 0.2f, 1f),
                Damage = 3f,
                ArmorPen = 0f,
                TopSpeed = 0.4f
            };
        }
        public static SpaceObject MDAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["WhiteBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.6f, 0.3f, 1f),
                Damage = 5f,
                ArmorPen = 0.01f,
                TopSpeed = 0.6f
            };
        }
        public static SpaceObject LDAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["WhiteBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(1f, 0.4f, 1f),
                Damage = 10f,
                ArmorPen = 0.02f,
                TopSpeed = 0.7f
            };
        }
        public static SpaceObject SPAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["GreenBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.2f, 0.2f, 1f),
                Damage = 0f,
                ArmorPen = 0.05f,
                TopSpeed = 0.5f
            };
        }
        public static SpaceObject MPAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["GreenBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.4f, 0.4f, 1f),
                Damage = 2,
                ArmorPen = 0.08f,
                TopSpeed = 0.6f
            };
        }
        public static SpaceObject LPAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["GreenBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.6f, 0.6f, 1f),
                Damage = 4f,
                ArmorPen = 0.1f,
                TopSpeed = 0.8f
            };
        }
        public static SpaceObject SSAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["BlueBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.2f, 0.8f, 1f),
                Damage = 1f,
                ArmorPen = 0.01f,
                TopSpeed = 0.8f
            };
        }
        public static SpaceObject MSAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["BlueBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.2f, 1.2f, 1f),
                Damage = 2f,
                ArmorPen = 0.02f,
                TopSpeed = 1.0f
            };
        }
        public static SpaceObject LSAmmo()
        {
            return new SpaceObject()
            {
                RenderSections = new List<RenderObject.Section>() { Program.RenderSections["BlueBullet"] },
                Shader = Program.Shaders["texture_shader"],
                Scale = new Vector3(0.2f, 1.8f, 1f),
                Damage = 3f,
                ArmorPen = 0.03f,
                TopSpeed = 1.2f
            };
        }
    }
}
