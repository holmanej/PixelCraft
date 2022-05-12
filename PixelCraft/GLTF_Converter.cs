using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelCraft
{
    class GLTF_Converter
    {
        public class GLTF_Loader
        {
            public Dictionary<string, string> asset;
            public int scene;
            public List<Scene> scenes;
            public List<Node> nodes;
            public List<Material> materials;
            public List<Image> images;
            public List<Texture> textures;
            public List<Mesh> meshes;
            public List<Accessor> accessors;
            public List<BufferView> bufferViews;
            public List<Buffer> buffers;
        }

        public class Scene
        {
            public List<int> nodes;
        }

        public class Node
        {
            public string name;
            public int mesh;
        }

        public class PBRMetallicRoughness
        {
            public class BaseColorTexture
            {
                public int index;
            }

            public List<float> baseColorFactor;
            public BaseColorTexture baseColorTexture;
            public float metallicFactor;
            public float roughnessFactor;
        }

        public class Material
        {
            public PBRMetallicRoughness pbrMetallicRoughness;
            public string name;
            public bool doubleSided;
        }

        public class Image
        {
            public int bufferView = -1;
            public string mimeType;
        }

        public class Texture
        {
            public int source;
            public int sampler;
        }

        public class Mesh
        {
            public class Primitive
            {
                public class Attribute
                {
                    public int POSITION;
                    public int NORMAL;
                    public int TEXCOORD_0;
                }
                public Attribute attributes;
                public int indices;
                public int mode;
                public int material;
            }

            public List<Primitive> primitives;
        }

        public class Accessor
        {
            public int bufferView;
            public int componentType;
            public int count;
            public string type;
            public int byteOffset;
            public List<float> min;
            public List<float> max;
        }

        public class BufferView
        {
            public int buffer;
            public int byteOffset;
            public int byteLength;
            public int target;
            public int byteStride;
        }

        public class Buffer
        {
            public string uri;
            public int byteLength;
        }

        private GLTF_Loader ModelData = new GLTF_Loader();

        public GLTF_Converter(string path)
        {
            Stopwatch sw = new Stopwatch();
            using (StreamReader streamReader = File.OpenText(path))
            {
                sw.Start();
                //Debug.WriteLine("Reading File");
                ModelData = JsonConvert.DeserializeObject<GLTF_Loader>(streamReader.ReadToEnd());
                sw.Stop();
            }

            Debug.WriteLine("Deserialize: " + sw.Elapsed);
            {
                //Debug.WriteLine("Asset: " + ModelData.asset["version"]);
                //Debug.WriteLine("Generator: " + ModelData.asset["generator"]);
                //Debug.WriteLine("Scene cnt: " + ModelData.scene);
                //Debug.WriteLine("Scene-Node: " + ModelData.scenes[0].nodes[0]);
                //Debug.WriteLine("Node Name: " + ModelData.nodes[0].name);
                //Debug.WriteLine("Node Mesh: " + ModelData.nodes[0].mesh);
                //Debug.WriteLine("Material Count: " + ModelData.materials.Count);
                //Debug.WriteLine("Material Name: " + ModelData.materials[0].name);
                //Debug.WriteLine("Material DblS: " + ModelData.materials[0].doubleSided);
                //Debug.WriteLine("Material-tex-index: " + ModelData.materials[1].pbrMetallicRoughness.baseColorTexture.index);
                //Debug.Write("Colors: ");
                //ModelData.materials[0].pbrMetallicRoughness.baseColorFactor.ForEach(n => Debug.Write(n + ", "));
                //Debug.WriteLine("\r\nMetallic Factor: " + ModelData.materials[0].pbrMetallicRoughness.metallicFactor);
                //Debug.WriteLine("Roughness Factor: " + ModelData.materials[0].pbrMetallicRoughness.roughnessFactor);
                //Debug.WriteLine("Image-bufferView: " + ModelData.images[0].bufferView);
                //Debug.WriteLine("Image-mimeType: " + ModelData.images[0].mimeType);
                //Debug.WriteLine("Texture-source: " + ModelData.textures[0].source);
                //Debug.WriteLine("Texture-sampler: " + ModelData.textures[0].sampler);
                //Debug.WriteLine("Mesh Attribute Position: " + ModelData.meshes[0].primitives[0].attributes.POSITION);
                //Debug.WriteLine("Mesh Attribute Normal: " + ModelData.meshes[0].primitives[0].attributes.NORMAL);
                //Debug.WriteLine("Mesh Attribute TexCoord: " + ModelData.meshes[0].primitives[1].attributes.TEXCOORD_0);
                //Debug.WriteLine("Mesh Indicies: " + ModelData.meshes[0].primitives[0].indices);
                //Debug.WriteLine("Mesh Mode: " + ModelData.meshes[0].primitives[0].mode);
                //Debug.WriteLine("Mesh Material: " + ModelData.meshes[0].primitives[0].material);
                //Debug.WriteLine("Accessor Bufferview: " + ModelData.accessors[0].bufferView);
                //Debug.WriteLine("Accessor CompType: " + ModelData.accessors[0].componentType);
                //Debug.WriteLine("Accessor Count: " + ModelData.accessors[0].count);
                //Debug.WriteLine("Accessor Type: " + ModelData.accessors[0].type);
                //Debug.Write("Min: ");
                //ModelData.accessors[0].min.ForEach(n => Debug.Write(n + ", "));
                //Debug.Write("Max: ");
                //ModelData.accessors[0].max.ForEach(n => Debug.Write(n + ", "));
                //Debug.WriteLine("BufferView Count: " + ModelData.bufferViews.Count);
                //foreach (Dictionary<string, int> b in ModelData.bufferViews)
                //{
                //    Debug.WriteLine("Buffer: " + b["buffer"]);
                //    Debug.WriteLine("Bufferlength: " + b["byteLength"]);
                //    Debug.WriteLine("Target: " + b["target"]);
                //}

                //Debug.WriteLine("URI: " + ModelData.buffers[0].uri);
                //Debug.WriteLine("Byte Length: " + ModelData.buffers[0].byteLength);
            }

        }

        public List<GameObject.Section> GetBufferData()
        {
            List<GameObject.Section> sections = new List<GameObject.Section>();

            foreach (Mesh m in ModelData.meshes)
            {
                foreach (Mesh.Primitive p in m.primitives)
                {
                    GameObject.Section section = new GameObject.Section();
                    List<float> vboData = new List<float>();
                    List<float> vertices = (List<float>)GetBufferData(p.attributes.POSITION);
                    List<float> normals = (List<float>)GetBufferData(p.attributes.NORMAL);
                    List<float> colors = new List<float>() { 1f, 1f, 1f, 1f };
                    PBRMetallicRoughness pbr = ModelData.materials[p.material].pbrMetallicRoughness;

                    section.metal = pbr.metallicFactor;
                    section.rough = pbr.roughnessFactor;
                    if (pbr.baseColorFactor != null)
                    {
                        colors = pbr.baseColorFactor;
                    }

                    List<float> texCoords = new List<float>();
                    if (p.attributes.TEXCOORD_0 != 0)
                    {
                        texCoords = (List<float>)GetBufferData(p.attributes.TEXCOORD_0);
                        byte[] imageData = GetBufferImage(pbr.baseColorTexture.index);

                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            Bitmap bmp = new Bitmap(ms);
                            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            section.ImageData = new byte[bmp.Width * bmp.Height * 4];
                            Marshal.Copy(bmpData.Scan0, section.ImageData, 0, section.ImageData.Length);
                            bmp.UnlockBits(bmpData);
                            section.ImageSize = bmp.Size;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < vertices.Count; i++)
                        {
                            texCoords.Add(0);
                        }
                    }
                    List<int> indices = new List<int>();
                    foreach (var n in GetBufferData(p.indices))
                    {
                        indices.Add(Convert.ToInt32(n));
                    }
                    vboData.AddRange(OrderByIndices(vertices, normals, colors, texCoords, indices));
                    section.VBOData = vboData;

                    sections.Add(section);
                }
            }
            return sections;
        }

        public byte[] GetBufferImage(int bufferViewID)
        {
            BufferView bf = ModelData.bufferViews[bufferViewID];
            int bufferID = bf.buffer;
            string uri = ModelData.buffers[bufferID].uri;
            uri = uri.Substring(uri.LastIndexOf(',') + 1);
            byte[] data = Convert.FromBase64String(uri);
            List<byte> img = new List<byte>();
            for (int i = bf.byteOffset; i < bf.byteOffset + bf.byteLength; i++)
            {
                img.Add(data[i]);
            }
            return img.ToArray();
        }

        private IList GetBufferData(int accessorID)
        {
            int bufferViewID = ModelData.accessors[accessorID].bufferView;
            int componentType = ModelData.accessors[accessorID].componentType;
            string dataType = ModelData.accessors[accessorID].type;
            IList bufferData;
            int dataSize;
            int typeSize;

            switch (componentType)
            {
                case 5120:
                    bufferData = new List<sbyte>();
                    dataSize = sizeof(byte);
                    break;
                case 5121:
                    bufferData = new List<byte>();
                    dataSize = sizeof(sbyte);
                    break;
                case 5122:
                    bufferData = new List<short>();
                    dataSize = sizeof(short);
                    break;
                case 5123:
                    bufferData = new List<ushort>();
                    dataSize = sizeof(ushort);
                    break;
                case 5125:
                    bufferData = new List<uint>();
                    dataSize = sizeof(uint);
                    break;
                default:
                    bufferData = new List<float>();
                    dataSize = sizeof(float);
                    break;
            }

            switch (dataType)
            {
                case "SCALAR":
                    typeSize = 1;
                    break;
                case "VEC2":
                    typeSize = 2;
                    break;
                default:
                    typeSize = 3;
                    break;
            }

            BufferView bf = ModelData.bufferViews[bufferViewID];
            int bufferID = bf.buffer;
            string uri = ModelData.buffers[bufferID].uri;
            uri = uri.Substring(uri.LastIndexOf(',') + 1);
            byte[] data = Convert.FromBase64String(uri);

            int index = bf.byteOffset + ModelData.accessors[accessorID].byteOffset;
            int target = index + (ModelData.accessors[accessorID].count * dataSize * typeSize);
            for (; index < target; index += dataSize)
            {
                switch (componentType)
                {
                    case 5120:
                        bufferData.Add(data[index]);
                        break;
                    case 5121:
                        bufferData.Add(data[index]);
                        break;
                    case 5122:
                        bufferData.Add(BitConverter.ToInt16(data, index));
                        break;
                    case 5123:
                        bufferData.Add(BitConverter.ToUInt16(data, index));
                        break;
                    case 5125:
                        bufferData.Add(BitConverter.ToInt32(data, index));
                        break;
                    default:
                        bufferData.Add(BitConverter.ToSingle(data, index));
                        break;
                }
            }

            return bufferData;
        }

        private List<float> OrderByIndices(List<float> v, List<float> n, List<float> c, List<float> t, List<int> indices)
        {
            List<float> orderedData = new List<float>();

            for (int i = 0; i < indices.Count; i++)
            {
                orderedData.AddRange(v.GetRange(indices[i] * 3, 3));
                orderedData.AddRange(n.GetRange(indices[i] * 3, 3));
                orderedData.AddRange(c);
                orderedData.AddRange(t.GetRange(indices[i] * 2, 2));
            }

            return orderedData;
        }
    }
}
