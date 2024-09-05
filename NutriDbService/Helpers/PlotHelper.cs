using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NutriDbService.Helpers
{
    public class PlotHelper
    {
        private string token = "6719978038:AAFFTz8Tat9ieYUyCp-tI2RAbcpddqTcZNY";
        private readonly ILogger _logger;
        public PlotHelper(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TransmitterHelper>>(); ;
        }
        public void SendPlot(decimal[] values, string[] labels, long userTgId)
        {
            var filepath = $"{Guid.NewGuid().ToString()}.png";
            CreateBarChart(values, labels, filepath);
            SendPhotoAsync(userTgId, filepath).GetAwaiter().GetResult();
            System.IO.File.Delete(filepath);
        }
        private async Task SendPhotoAsync(long chatId, string filePath)
        {
            try
            {
                var botClient = new TelegramBotClient(token);

                // Убедитесь, что файл существует
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                // Чтение файла изображения
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var inputOnlineFile = new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream, Path.GetFileName(filePath));

                // Отправка изображения
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: inputOnlineFile,
                    caption: string.Empty
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
                throw;
            }
        }
        static void CreateBarChart(decimal[] values, string[] labels, string filePath)
        {
            int width = 600;
            int height = 400;
            int barWidth = 60;
            int spaceBetweenBars = 20;
            int chartHeight = 300;

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            using var paint = new SKPaint
            {
                TextSize = 10,
                IsAntialias = true,
                Color = new SKColor(0, 0, 0),
                IsStroke = false
            };

            canvas.Clear(SKColors.White);

            decimal minValue = 0;
            decimal maxValue = decimal.MinValue;
            foreach (decimal value in values)
            {
                if (value > maxValue) maxValue = value;
            }

            for (int i = 0; i < values.Length; i++)
            {
                decimal value = values[i];
                decimal proportion = value / maxValue;
                int barHeight = (int)(proportion * chartHeight);
                int x = (i * (barWidth + spaceBetweenBars)) + spaceBetweenBars;
                int y = height - barHeight - 50;

                using var barPaint = new SKPaint
                {
                    Color = SKColors.Blue,
                    IsStroke = false
                };
                canvas.DrawRect(x, y, barWidth, barHeight, barPaint);

                string label = labels[i];
                canvas.DrawText(label, x, height - 45, paint);

                string valueLabel = value.ToString("0.0");
                float valueLabelWidth = paint.MeasureText(valueLabel);
                float valueX = x + (barWidth / 2) - (valueLabelWidth / 2);
                float valueY = y - 20;
                canvas.DrawText(valueLabel, valueX, valueY, paint);
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(filePath);
            data.SaveTo(stream);
        }
    
    //private void CreateBarChart(decimal[] values, string[] labels, string filePath)
    //{
    //    try
    //    {
    //        int width = 600;
    //        int height = 400;
    //        int barWidth = 60;
    //        int spaceBetweenBars = 20;
    //        int chartHeight = 300;

    //        using var bitmap = new Bitmap(width, height);
    //        using var graphics = Graphics.FromImage(bitmap);
    //        using var font = new Font("Arial", 10);

    //        graphics.Clear(System.Drawing.Color.White);

    //        decimal minValue = 0;
    //        decimal maxValue = decimal.MinValue;
    //        foreach (decimal value in values)
    //        {
    //            if (value > maxValue) maxValue = value;
    //        }

    //        for (int i = 0; i < values.Length; i++)
    //        {
    //            decimal value = values[i];
    //            decimal proportion = value / maxValue;
    //            int barHeight = (int)(proportion * chartHeight);
    //            int x = (i * (barWidth + spaceBetweenBars)) + spaceBetweenBars;
    //            int y = height - barHeight - 50;

    //            using var brush = new SolidBrush(System.Drawing.Color.Blue);
    //            graphics.FillRectangle(brush, x, y, barWidth, barHeight);

    //            string label = labels[i];
    //            graphics.DrawString(label, font, Brushes.Black, x, height - 45);

    //            string valueLabel = value.ToString("0.0");
    //            float valueLabelWidth = graphics.MeasureString(valueLabel, font).Width;
    //            float valueX = x + (barWidth / 2) - (valueLabelWidth / 2);
    //            float valueY = y - 20;
    //            graphics.DrawString(valueLabel, font, Brushes.Black, valueX, valueY);
    //        }
    //        bitmap.Save(filePath, ImageFormat.Png);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
    //        throw;
    //    }
    //}
}

}
