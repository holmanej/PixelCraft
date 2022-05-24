using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    public class TextObject : RenderObject
    {
        public RenderObject FontSet;
        public string _Text;

        public TextObject(string text, RenderObject fontset, Shader shader)
        {
            FontSet = fontset;
            Shader = shader;
            _Text = text;
            WriteString();
        }

        private void WriteString()
        {
            var chars = new List<Section>();
            float width = 0;
            for (int i = 0; i < _Text.Length; i++)
            {
                var c = FontSet.RenderSections[_Text[i] - ' '];
                int cx = c.ImageSize.Width;
                int cy = c.ImageSize.Height;
                chars.Add(new Section()
                {
                    VBOData = new List<float>()
                    {
                        width,      0,  0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                        width + cx, 0,  0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                        width,      cy, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        width + cx, 0,  0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                        width,      cy, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        width + cx, cy,  0, 0, 0, 0, 0, 0, 0, 0, 1, 0
                    },
                    ImageData = c.ImageData,
                    ImageSize = c.ImageSize,
                    ImageHandle = c.ImageHandle,
                    ImageUpdate = true,
                    Metal = 0.5f,
                    Rough = 0.5f
                });
                width += cx;
                //height = Math.Max(height, cy);
            }

            RenderSections = chars;
        }

        public string Text
        {
            get { return _Text; }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    WriteString();
                }
            }
        }
    }
}
