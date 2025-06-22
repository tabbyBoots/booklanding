using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace mvcDapper3.AppCodes.AppService
{
    public class CaptchaService
    {
        private readonly Random _random = new();
        private const int Width = 150;
        private const int Height = 50;
        private const int FontSize = 20;
        private const int TextLength = 5;
        private readonly Font _font;

        public CaptchaService()
        {
            var fontCollection = new FontCollection();
            _font = SystemFonts.CreateFont("Arial", FontSize);
        }

        public (string Code, byte[] ImageBytes) GenerateCaptcha()
        {
            // Generate random code
            var code = GenerateRandomCode();
            
            using var image = new Image<Rgba32>(Width, Height);
            
            // Set background
            image.Mutate(ctx => ctx.Fill(Color.White));
            
            // Add noise
            AddNoise(image);
            
            // Draw text
            DrawText(image, code);
            
            // Convert to byte array
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            
            return (code, stream.ToArray());
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            return new string(Enumerable.Repeat(chars, TextLength)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        private void AddNoise(Image<Rgba32> image)
        {
            image.Mutate(ctx => 
            {
                for (var i = 0; i < 50; i++)
                {
                    var x = _random.Next(Width);
                    var y = _random.Next(Height);
                    ctx.DrawLine(Color.LightGray, 1, new PointF(x, y), new PointF(x + 1, y + 1));
                }
            });
        }

        private void DrawText(Image<Rgba32> image, string code)
        {
            image.Mutate(ctx => 
            {
                for (var i = 0; i < code.Length; i++)
                {
                    var x = 10 + i * 25;
                    var y = _random.Next(5, 15);
                    var angle = _random.Next(-15, 15);
                    
                    var textOptions = new DrawingOptions
                    {
                        Transform = Matrix3x2Extensions.CreateRotationDegrees(angle, new PointF(x, y))
                    };
                    
                    ctx.DrawText(textOptions, code[i].ToString(), _font, Color.Black, new PointF(x, y));
                }
            });
        }
    }
}
