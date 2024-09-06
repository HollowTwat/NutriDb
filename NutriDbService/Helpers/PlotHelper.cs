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
            filePath = System.IO.Path.Combine(homePath, "barchart.png");


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
    //public static void CreateBarChart(decimal[] values, string[] labels, string filePath)
    //{
    //    int width = 600;
    //    int height = 400;
    //    int margin = 40;
    //    int barWidth = (width - 2 * margin) / values.Length;
    //    int maxHeight = height - 2 * margin;

    //    decimal maxValue = values.Max();

    //    // Создание битмапа 
    //    using var image = new Image<Rgba32>(width, height);

    //    // Добавим fill и draw
    //    var options = new DrawingOptions()
    //    {
    //        GraphicsOptions = new GraphicsOptions()
    //        {
    //            Antialias = true
    //        }
    //    };

    //    // Создание шрифта
    //    var fontCollection = new FontCollection();
    //    var fontFamily = fontCollection.Add("Arial.ttf"); // Используйте корректный путь к шрифту, или платформенные системные шрифты.
    //    Font font = fontFamily.CreateFont(11);

    //    image.Mutate(ctx =>
    //    {
    //        // Заполнение фона белым цветом
    //        ctx.Fill(Color.White);

    //        // Нарисуйте оси
    //        ctx.DrawLine(Color.Black, 2, new PointF[]
    //        {
    //        new PointF(margin, height - margin),
    //        new PointF(width - margin, height - margin)
    //        }); // X-Oсь

    //        ctx.DrawLine(Color.Black, 2, new PointF[]
    //        {
    //        new PointF(margin, height - margin),
    //        new PointF(margin, margin)
    //        }); // Y-Ось

    //        for (int i = 0; i < values.Length; i++)
    //        {
    //            // Определение высоты баров
    //            float barHeight = (float)(values[i] / maxValue * maxHeight);
    //            var barRectangle = new RectangularPolygon(
    //                x: margin + i * barWidth,
    //                y: height - margin - barHeight,
    //                width: barWidth - 10,
    //                height: barHeight);

    //            ctx.Fill(Color.LightBlue, barRectangle);
    //            ctx.DrawText(labels[i], font, Color.Black, new PointF(margin + i * barWidth + (barWidth - 10) / 4, height - margin + 5));
    //        }
    //    });

    //    // Сохранить битмап в файл
    //    using var fileStream = new FileStream(filePath, FileMode.Create);
    //    image.SaveAsPng(fileStream);
    //}
}
    //public static void CreateBarChart(decimal[] values, string[] labels, string filePath)
    //{
    //    // Создаем модель графика
    //    var plotModel = new PlotModel { Title = "Bar Chart" };

    //    // Определяем ось X как категорию
    //    var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom, Angle = 45 };
    //    categoryAxis.Labels.AddRange(labels);
    //    plotModel.Axes.Add(categoryAxis);

    //    // Определяем ось Y как число
    //    var valueAxis = new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 0, AbsoluteMinimum = 0 };
    //    plotModel.Axes.Add(valueAxis);

    //    // Создаем столбчатую диаграмму и добавляем значения
    //    var barSeries = new BarSeries();
    //    for (int i = 0; i < values.Length; i++)
    //    {
    //        barSeries.Items.Add(new BarItem { Value = (double)values[i] });
    //    }
    //    plotModel.Series.Add(barSeries);

    //    // Убедитесь, что путь к файлу существует и папки все доступны
    //    //Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), filePath));


    //    // Экспортируем диаграмму в формате PNG
    //    PngExporter.Export(plotModel, filePath, 600, 400);
    //}


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

