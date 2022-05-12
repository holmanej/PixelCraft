using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    class GLTFObject : GameObject
    {
        public GLTFObject(GLTF_Converter gltf, Shader shader)
        {
            RenderSections = gltf.GetBufferData();
            RenderSections.Sort((a, b) => b.VBOData[9].CompareTo(a.VBOData[9]));
            Shader = shader;
        }
    }
}
