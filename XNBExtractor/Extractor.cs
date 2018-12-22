using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XNBExtractor
{
    public sealed class Extractor : IDisposable
    {
        public enum AssetType
        {
            Texture2D,
            TextureCube,
            MusicOrSound,
        }


        public enum ImageExtension { Png, Jpeg }


        public ImageExtension SaveImagesAs { get; set; }


        public static GraphicsProfile GraphicsProfile
        {
            get => GraphicsDeviceService.GraphicsProfile;
            set => GraphicsDeviceService.GraphicsProfile = value;
        }


        public bool Initialized { get; private set; } = false;


        private readonly List<string> filesToDelete = new List<string>();


        private GraphicsDeviceService graphicsDeviceService;


        private ContentManager content;

        
        public void Initialize()
        {
            if (Initialized)
                return;

            Initialized = true;
            
            var form = new Form();
            graphicsDeviceService = GraphicsDeviceService.AddRef(form.Handle, form.ClientSize.Width, form.ClientSize.Height);
            var services = new ServiceContainer();
            services.AddService<IGraphicsDeviceService>(graphicsDeviceService);
            content = new ContentManager(services);
        }

        
        public void ProcessFiles(AssetType assetType, params string[] files)
        {
            if (!Initialized)
                return;

            Action<FileInfo> convertAction;

            switch (assetType)
            {
                case AssetType.Texture2D:
                    convertAction = ConvertTexture2DToImage;
                    break;
                case AssetType.TextureCube:
                    convertAction = ConvertTextureCubeToImage;
                    break;
                case AssetType.MusicOrSound:
                    throw new NotImplementedException();
                default:
                    Console.WriteLine("Unsupported asset type.");
                    return;
            }

            Console.WriteLine($"Asset type is set to {assetType.ToString()}");

            foreach (var file in files)
            {
                Console.WriteLine($"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} - {file}");

                var info = new FileInfo(file);

                if (info.Exists)
                {
                    if (info.Extension.ToLower() == ".xnb")
                        convertAction(info);
                    else
                        Console.WriteLine($"Invalid file extension - .xnb expected, got {info.Extension} instead.");
                }
                else
                    Console.WriteLine($"File doesn't exist: {file}");
            }

            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught exception while deleting files:");
                    Console.WriteLine(e.Message);
                }
            }
        }

        
        private void ConvertTexture2DToImage(FileInfo file)
        {
            try
            {
                var fileCopy = file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.Name.Replace(".xnb", ".tmp.xnb")));
                var assetName = $"{file.Name}.tmp.xnb";
                filesToDelete.Add(fileCopy.FullName);
                var texture = content.Load<Texture2D>(fileCopy.Name.Replace(".xnb", ""));

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
            catch (Exception e)
            {
                Console.WriteLine("Caught exception while converting Texture2D to Image:");
                Console.WriteLine(e.Message);
            }
        }
        
        
        private void ConvertTextureCubeToImage(FileInfo file)
        {
            try
            {
                var fileCopy = file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.Name.Replace(".xnb", ".tmp.xnb")));
                var assetName = $"{file.Name}.tmp.xnb";
                filesToDelete.Add(fileCopy.FullName);
                var cube = content.Load<TextureCube>(fileCopy.Name.Replace(".xnb", ""));

                using (var texture = new Texture2D(graphicsDeviceService.GraphicsDevice, cube.Size, cube.Size))
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
                Console.WriteLine("Caught exception while converting TextureCube to images:");
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


        public void Dispose()
        {
            ((IDisposable)content).Dispose();
            graphicsDeviceService.Release(true);
        }
    }
}
