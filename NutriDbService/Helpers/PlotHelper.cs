using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using SixLabors.Fonts;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using System.Reflection;
using System.Numerics;

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
            string filePath = $"{Guid.NewGuid().ToString()}.png";

            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            filePath = System.IO.Path.Combine(homePath, filePath);


            CreateBarChart(values, labels, filePath);
            SendPhotoAsync(userTgId, filePath).GetAwaiter().GetResult();
            System.IO.File.Delete(filePath);
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
                var inputOnlineFile = new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream, System.IO.Path.GetFileName(filePath));

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
        public static void CreateBarChart(decimal[] values, string[] labels, string filePath)
        {
            int width = 600;
            int height = 400;
            int margin = 40;
            int barWidth = (width - 2 * margin) / values.Length;
            int maxHeight = height - 2 * margin;

            decimal maxValue = values.Max();

            // Создание битмапа 
            using var image = new Image<Rgba32>(width, height);

            // Настройки кисти и пера
            var pen = Pens.Solid(Color.Black, 2);

            // Загружаем системный шрифт
            var fontCollection = new FontCollection();

            // Change FontFamily name as per your system installed fonts
            //FontFamily fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == "Arial"); //?? SystemFonts.Collection.AddSystemFontCollection().Families.First();
            //Font font = fontFamily.CreateFont(11, SixLabors.Fonts.FontStyle.Regular);

            var fontPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Roboto-Black.ttf");
            FontFamily fontFamily = fontCollection.Add(fontPath);
            Font font = fontFamily.CreateFont(11, FontStyle.Regular);
            Font axisFont = fontFamily.CreateFont(12, FontStyle.Bold);
            image.Mutate(ctx =>
            {
                // Заполнение фона белым цветом
                ctx.Fill(Color.White);

                // Нарисуйте оси
                ctx.DrawLine(pen, new PointF[]
                {
                new PointF(margin, height - margin),
                new PointF(width - margin, height - margin)
                }); // X-Ось

                ctx.DrawLine(pen, new PointF[]
                {
                new PointF(margin, height - margin),
                new PointF(margin, margin)
                }); // Y-Ось

                // Получаем уникальные y значения и сортируем их
                var uniqueValues = values.Select(v => Math.Round(v)).Distinct().OrderBy(v => v).ToArray();
                foreach (var value in uniqueValues)
                {
                    float yPos = height - margin - ((float)value / (float)maxValue * maxHeight);
                    ctx.DrawLine(pen, new PointF(margin - 5, yPos), new PointF(margin + 5, yPos));

                    string label = value.ToString("0");
                    ctx.DrawText(label, font, Color.Black, new PointF(5, yPos - 10));
                }

                for (int i = 0; i < values.Length; i++)
                {
                    // Определение высоты баров
                    float barHeight = (float)(values[i] / maxValue * maxHeight);
                    var barRectangle = new RectangularPolygon(
                                      x: margin + i * barWidth,
                                      y: height - margin - barHeight,
                                      width: barWidth - 10,
                                      height: barHeight);

                    ctx.Fill(Color.LightBlue, barRectangle);
                    ctx.DrawText(labels[i], font, Color.Black, new PointF(margin + i * barWidth + (barWidth - 10) / 4, height - margin + 5));
                }
            });

            // Сохранение битмапа в файл
            using var fileStream = new FileStream(filePath, FileMode.Create);
            image.SaveAsPng(fileStream);
        }
    }
}





    // Рисуем вертикальную подпись оси Y
    //var yAxisText = "Frequency";
    //var size = TextMeasurer.MeasureAdvance(yAxisText, new TextOptions(axisFont));
    //float titleX = margin - 30;
    //float titleY = height / 2;

    //PointF origin = new PointF(titleX, titleY);
    //Matrix3x2 transformMatrix = Matrix3x2.CreateRotation(-MathF.PI / 2, origin);
    //var options = new DrawingOptions
    //{
    //    GraphicsOptions = new GraphicsOptions { Antialias = true },
    //    Transform = transformMatrix
    //};

