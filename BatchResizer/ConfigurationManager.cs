using FluentValidation;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace BatchResizer
{
    internal class Configuration
    {
        public int? Quality { get; set; } = 80;
        public int? Height { get; set; } = 480;

        public int? Width { get; set; } = 480;

        public ResizeMode? Mode { get; set; } = ResizeMode.Max;
        public bool? Grayscale { get; set; } = false;

    }
    internal class ConfigurationValidator :  AbstractValidator<Configuration>
    {
        public ConfigurationValidator()
        {
            RuleFor(opt => opt.Quality)
                .NotNull()
                .NotEmpty()
                .LessThanOrEqualTo(100)
                .GreaterThanOrEqualTo(50);

            RuleFor(opt => opt.Height)
                .NotNull()
                .NotEmpty()
                .LessThanOrEqualTo(2048)
                .GreaterThanOrEqualTo(256);


            RuleFor(opt => opt.Width)
                .NotNull()
                .NotEmpty()
                .LessThanOrEqualTo(2048)
                .GreaterThanOrEqualTo(256);

            RuleFor(opt => opt.Grayscale)
                .NotNull()
                .NotEmpty();

            RuleFor(opt => opt.Mode)
                .NotEmpty()
                .IsInEnum();
        }
    }

    internal class ConfigurationManager
    {
        private static Configuration instance;
        private static object sync = new();

        private ConfigurationManager() { }

        public static Configuration GetConfiguration()
        {
            if (instance == null)
            {
                var asm_path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName);

                lock (sync)
                {
                    try
                    {
                        instance = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(asm_path + "\\config.json"))!;

                        var validator = new ConfigurationValidator();
                        validator.ValidateAndThrow(instance);
                    }
                    catch (Exception ex) 
                    {
                        File.WriteAllText(asm_path + "\\last_error.log", ex.Message);
                        instance = new Configuration();
                    }
                }
            }
            return instance;
        }
    }

}
