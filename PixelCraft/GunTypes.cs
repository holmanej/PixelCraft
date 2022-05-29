using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    /* TYPE BST FRTE ACC SPRD RNGE ARC
     * FB   5   200  0   30   15   15
     * SB   3   400  0   45   10   90
     * GB   2   500  60  0    10   360
     * 
     * FR   1   50   15  0    20   15
     * SR   1   100  30  0    15   45
     * GR   1   200  45  0    15   360
     * 
     * FP   1   300  5   10   25   15
     * SP   1   400  15  5    20   60
     * GP   1   500  20  0    20   180
     * 
     * NOVA 45  2500 0   360  5    360
    */

    public static class GunTypes
    {
        public static SpaceObject FBGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 5,
                FireRate = 200,
                Accuracy = 0,
                Spread = 30,
                FiringArc = 15,
                Range = 15,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject SBGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 3,
                FireRate = 400,
                Accuracy = 0,
                Spread = 45,
                FiringArc = 90,
                Range = 10,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject GBGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 2,
                FireRate = 500,
                Accuracy = 60,
                Spread = 0,
                FiringArc = 360,
                Range = 10,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject FRGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 1,
                FireRate = 50,
                Accuracy = 15,
                Spread = 0,
                FiringArc = 15,
                Range = 20,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject SRGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 1,
                FireRate = 100,
                Accuracy = 30,
                Spread = 0,
                FiringArc = 45,
                Range = 15,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject GRGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 1,
                FireRate = 200,
                Accuracy = 45,
                Spread = 0,
                FiringArc = 360,
                Range = 15,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject FPGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 1,
                FireRate = 300,
                Accuracy = 5,
                Spread = 10,
                FiringArc = 15,
                Range = 25,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject SPGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 1,
                FireRate = 400,
                Accuracy = 15,
                Spread = 5,
                FiringArc = 60,
                Range = 20,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject GPGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 1,
                FireRate = 500,
                Accuracy = 25,
                Spread = 0,
                FiringArc = 180,
                Range = 20,
                Ammo = ammo,
                Armed = armed
            };
        }
        public static SpaceObject NovaGun(SpaceObject ammo = null, bool armed = true)
        {
            return new SpaceObject()
            {
                Burst = 45,
                FireRate = 2500,
                Accuracy = 0,
                Spread = 360,
                FiringArc = 360,
                Range = 5,
                Ammo = ammo,
                Armed = armed
            };
        }
    }
}
