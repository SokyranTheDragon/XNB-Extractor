using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNBExtractor.ExtensionMethods;

namespace XNBExtractor
{
    public class Extractor
    {
        public enum AssetType
        {
            Texture2D,
            TextureCube,
            MusicOrSound,
        }


        public enum ImageExtension { Png, Jpeg }


        public ImageExtension SaveImagesAs { get; set; }


        private readonly List<string> filesToDelete = new List<string>();


        public Extractor(Dictionary<string, AssetType> files)
        {
            var form = new Form();
            var graphicsDeviceService = GraphicsDeviceService.AddRef(form.Handle, form.ClientSize.Width, form.ClientSize.Height);
            var services = new ServiceContainer();
            services.AddService<IGraphicsDeviceService>(graphicsDeviceService);
            var content = new ContentManager(services);

            foreach (var (path, type) in files)
            {
                Console.WriteLine($"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} - {type} - {path}");

                var file = new FileInfo(path);

                if (file.Exists)
                {
                    if (file.Extension.ToLower() == ".xnb")
                    {
                        switch (type)
                        {
                            case AssetType.Texture2D:
                                ConvertTexture2DToPng(content, file);
                                break;
                            case AssetType.TextureCube:
                                ConvertTextureCubeToPng(content, file, graphicsDeviceService.GraphicsDevice);
                                break;
                            case AssetType.MusicOrSound:
                                throw new NotImplementedException();
                            default:
                                Console.WriteLine("Unsupported asset type.");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid file extension - .xnb expected, got {file.Extension} instead.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid file.");
                }
            }
        }


        private void ConvertTexture2DToPng(ContentManager content, FileInfo file)
        {
            try
            {
                var fileCopy = file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{file.Name}.tmp.xnb"));
                var assetName = $"{file.Name}.tmp.xnb";
                filesToDelete.Add(fileCopy.FullName);

                using (var texture = content.Load<Texture2D>(fileCopy.FullName))
                {
                    var extension = SaveImagesAs == ImageExtension.Png ? "png" : "jpeg";
                    var fileToSave = file.FullName.Replace(".xnb", $".{extension}");

                    if (File.Exists(fileToSave))
                    {
                        var result = MessageBox.Show($"{fileToSave} Already exists, replace it?", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            SaveImage(texture, fileToSave);
                        }
                    }
                    else
                    {
                        SaveImage(texture, fileToSave);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private void ConvertTextureCubeToPng(ContentManager content, FileInfo file, GraphicsDevice graphicsDevice)
        {
            try
            {
                var fileCopy = file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{file.Name}.tmp.xnb"));
                var assetName = $"{file.Name}.tmp.xnb";
                filesToDelete.Add(fileCopy.FullName);
                
                using (var cube = content.Load<TextureCube>(fileCopy.FullName))
                using (var texture = new Texture2D(graphicsDevice, cube.Size, cube.Size))
                {
                    var data = new int[cube.Size * cube.Size];

                    for (int i = 0; i < 6; i++)
                    {
                        cube.GetData((CubeMapFace)i, data);
                        texture.SetData(data);

                        var extension = SaveImagesAs == ImageExtension.Png ? "png" : "jpeg";
                        var fileToSave = file.FullName.Replace(".xnb", $"_{i}.{extension}");

                        if (File.Exists(fileToSave))
                        {
                            var result = MessageBox.Show($"{fileToSave} Already exists, replace it?", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                SaveImage(texture, fileToSave);
                            }
                        }
                        else
                        {
                            SaveImage(texture, fileToSave);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private void SaveImage(Texture2D texture, string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                if (SaveImagesAs == ImageExtension.Png)
                {
                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                }
                else
                {
                    texture.SaveAsJpeg(stream, texture.Width, texture.Height);
                }

                Console.WriteLine($"Created: {stream.Name}");
            }
        }
    }
}
