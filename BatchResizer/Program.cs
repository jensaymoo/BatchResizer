using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace BatchResizer
{
 
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var config = new JObject();
            config["quality"] = 80;
            config["width"] = 640;
            config["height"] = 640;
            config["grayscale"] = true;

            var config_path = Environment.CurrentDirectory + "\\config.json";

            //if (File.Exists(config_path))
            //    config = JObject.Parse(File.ReadAllText(config_path));
            
            //File.WriteAllText(config_path, config.ToString());

            var jpeg_encoder = new JpegEncoder()
            {
                ColorType = JpegEncodingColor.YCbCrRatio420,
                Quality = (int)config["quality"],
                SkipMetadata = true
            };

            var resize_opt = new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size((int)config["width"], (int)config["height"])
            };

            List<string> not_conveted = new List<string>();
            using (var dialog = new FolderBrowserDialog())
            {
                var result_browser = dialog.ShowDialog();
                if (result_browser == DialogResult.OK)
                {
                    var dir_info = new DirectoryInfo(dialog.SelectedPath);
                    Parallel.ForEach(dir_info.GetFiles("*.jpg"), file =>
                    {
                        try
                        {
                            using (var image = Image.Load(file.FullName))
                            {
                                image.Metadata.ExifProfile = null;

                                if ((bool)config["grayscale"])
                                    image.Mutate(i => i.Resize(resize_opt).Grayscale());
                                else
                                    image.Mutate(i => i.Resize(resize_opt));

                                image.Save(file.FullName, jpeg_encoder);
                            }
                        }
                        catch
                        {
                            not_conveted.Add(file.Name);
                        }
                    });

                    if (not_conveted.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"При конвертации изображений в каталоге \"{dialog.SelectedPath}\" возникли сложности, следующие файлы не были преобразованы: \n");

                        foreach (var error in not_conveted)
                            sb.AppendLine(error);

                        MessageBox.Show(sb.ToString(), "Возникли некоторые сложности...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }
    }
}