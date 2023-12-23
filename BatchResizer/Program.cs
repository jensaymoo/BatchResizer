using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace BatchResizer
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Configuration config = ConfigurationManager.GetConfiguration();

            var jpeg_encoder = new JpegEncoder()
            {
                ColorType = JpegEncodingColor.YCbCrRatio420,
                Quality = config.Quality,
                SkipMetadata = true
            };

            var resize_opt = new ResizeOptions
            {
                Mode = config.Mode,
                Size = new Size(config.Width, config.Height)
            };

            //собираем список файлов
            List<string> files = new List<string>();
            foreach (var path in args)
            {
                if (File.Exists(path))
                {
                    if (Path.GetExtension(path).ToUpper() == ".JPG")
                        files.Add(path);
                }

                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var found_files = dir.GetFiles("*.JPG").Select(d => d.FullName);

                    files.AddRange(found_files);
                }
            }

            Parallel.ForEach(files.Distinct(), file =>
            {
                try
                {
                    using (var image = Image.Load(file))
                    {
                        ExifProfile profile = new ExifProfile();

                        var exif_tags = image.Metadata.ExifProfile?.Values
                            .Where(d => !d.Equals(ExifTag.Orientation))
                            .Select(v => v.Tag)
                            .ToList();

                        exif_tags?.ForEach(tag =>
                        {
                            image.Metadata.ExifProfile!.RemoveValue(tag);
                        });

                        image.Mutate(i => i.Resize(resize_opt));


                        if (config.Grayscale)
                            image.Mutate(i => i.Grayscale());

                        image.Save(file, jpeg_encoder);
                    }
                }
                catch
                {
                    return;
                }
            });
        }
    }
}