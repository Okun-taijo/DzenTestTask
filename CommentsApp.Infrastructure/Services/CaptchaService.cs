using System.Collections.Concurrent;
using CommentsApp.Application.DTOs;
using CommentsApp.Application.Interfaces;
using SkiaSharp;
namespace CommentsApp.Infrastructure.Services;
public class CaptchaService: ICaptchaService
{
    private readonly ConcurrentDictionary<string, string> _store = new();

    public  CaptchaResult Generate()
    {
        var code=GenerateCode();
        var id = Guid.NewGuid().ToString();

        _store[id]=code;
        
        var image = GenerateImage(code);

        return new CaptchaResult
        {
            CaptchaId = id,
            ImageBase64 = image
        };
    }

    public bool Validate(string captchaId, string userInput)
    {
        if(!_store.TryGetValue(captchaId,out var real))
            return false;
        
        return string.Equals(real, userInput, StringComparison.OrdinalIgnoreCase);
    }


    private string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        var random = new Random();

        return new string(Enumerable.Range(0,5).Select(c=>chars[random.Next(chars.Length)]).ToArray());
    }

    private string GenerateImage(string code)
    {
        const int width = 160;
        const int height = 60;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        var paint = new SKPaint
        {
            TextSize = 32,
            IsAntialias = true,
            Color = SKColors.Black,
            Typeface = SKTypeface.Default
        };

        var rnd = new Random();
        for (int i = 0; i < 5; i++)
        {
            var noisePaint = new SKPaint
            {
                Color = SKColors.LightGray,
                StrokeWidth = 2
            };

            canvas.DrawLine(
                rnd.Next(width), rnd.Next(height),
                rnd.Next(width), rnd.Next(height),
                noisePaint);
        }

        for (int i = 0; i < code.Length; i++)
        {
            var x = 20 + i * 25 + rnd.Next(-3, 3);
            var y = 40 + rnd.Next(-5, 5);

            canvas.DrawText(code[i].ToString(), x, y, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);

        return Convert.ToBase64String(data.ToArray());
    }
}