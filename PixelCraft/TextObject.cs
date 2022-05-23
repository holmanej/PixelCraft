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
        private FontFamily FontFamily;
        private List<Bitmap> Characters = new List<Bitmap>();

        public string _Text;
        public int _Size;
        public Color _Color;
        public Color _BGColor;

        public TextObject(string text, FontFamily fontfamily, Shader shader)
        {
            Shader = shader;
            _Text = text;
            _Size = 12;
            _Color = Color.Black;
            _BGColor = Color.White;
            FontFamily = fontfamily;

            CreateBitmaps();
            RenderSections = new List<Section>() { new Section() };
            WriteString();
        }

        private void CreateBitmaps()
        {
            Characters.Clear();
            Font font = new Font(FontFamily, _Size, GraphicsUnit.Pixel);
            for (int i = 32; i < 127; i++)
            {
                Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);
                SizeF charSize = g.MeasureString(Convert.ToChar(i).ToString(), font);
                Bitmap charBmp = new Bitmap((int)charSize.Width, font.Height, PixelFormat.Format32bppRgb);
                g = Graphics.FromImage(charBmp);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.Clear(_BGColor);
                g.DrawString(Convert.ToChar(i).ToString(), font, new SolidBrush(_Color), 0, 0, StringFormat.GenericTypographic);
                Characters.Add(charBmp);
            }
        }

        private void WriteString()
        {
            List<Bitmap> stringBmps = new List<Bitmap>();
            for (int i = 0; i < _Text.Length; i++)
            {
                stringBmps.Add(Characters[_Text[i] - ' ']);
            }
            int stringWidth = stringBmps.Sum(b => b.Width);
            int stringHeight = stringBmps[0].Height;

            Bitmap template = new Bitmap(stringWidth, stringHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(template))
            {
                int totalWidth = 0;
                for (int i = 0; i < stringBmps.Count; i++)
                {
                    g.DrawImage(stringBmps[i], totalWidth, 0);
                    totalWidth += stringBmps[i].Width;
                }
            }

            template.MakeTransparent(_BGColor);
            BitmapData charData = template.LockBits(new Rectangle(0, 0, template.Width, template.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            byte[] imgData = new byte[template.Width * template.Height * 4];
            Marshal.Copy(charData.Scan0, imgData, 0, imgData.Length);
            template.UnlockBits(charData);

            float w = stringWidth / 400f;
            float h = stringHeight / 300f;

            int handle = RenderSections[0].ImageHandle;
            RenderSections[0] = new Section()
            {
                VBOData = new List<float>()
                    {
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                        w, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                        0, h, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        w, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                        0, h, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        w, h, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0
                    },
                ImageData = imgData,
                ImageSize = template.Size,
                ImageHandle = handle,
                ImageUpdate = true,
                Metal = 0.5f,
                Rough = 0.5f
            };

            template.Dispose();
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

        public int Size
        {
            get { return _Size; }
            set
            {
                if (value != _Size)
                {
                    _Size = value;
                    CreateBitmaps();
                    WriteString();
                }
            }
        }

        public Color Color
        {
            get { return _Color; }
            set
            {
                if (value != _Color)
                {
                    _Color = value;
                    CreateBitmaps();
                    WriteString();
                }
            }
        }

        public Color BGColor
        {
            get { return _BGColor; }
            set
            {
                if (value != _BGColor)
                {
                    _BGColor = value;
                    CreateBitmaps();
                    WriteString();
                }
            }
        }
    }
}
