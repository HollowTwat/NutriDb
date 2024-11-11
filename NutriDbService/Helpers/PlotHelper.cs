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
            _logger = serviceProvider.GetRequiredService<ILogger<PlotHelper>>(); ;
        }
        public PlotHelper()
        {
            //_logger = serviceProvider.GetRequiredService<ILogger<TransmitterHelper>>(); ;
        }
        public async Task SendPlot(decimal[] values, string[] labels, long userTgId, decimal? goalkk)
        {
            string filePath = $"{Guid.NewGuid().ToString()}.png";

            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            filePath = System.IO.Path.Combine(homePath, filePath);


            CreateBarChart(values, labels, filePath, goalkk);
            await SendPhotoAsync(userTgId, filePath);
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
        public static void CreateBarChart(decimal[] values, string[] labels, string filePath, decimal? goalkk)
        {

            int width = 800;
            int height = 600;
            int margin = 150;
            int textFromAxe = 50;
            int barSpace = 20;
            int barWidth = (width - 2 * margin) / values.Length;
            int maxHeight = height - 2 * margin - 40;

            decimal goalDelta = 0.0m;
            decimal maxValue = values.Max();

            float goalYPos = 0;
            float goalHYPos = 0;
            float goalLYPos = 0;
            if (goalkk != null && goalkk > 0)
            {
                goalDelta = decimal.Round((decimal)goalkk * 0.1M, 0);
                if (goalkk > maxValue)
                    maxValue = (decimal)goalkk;
                goalkk = Decimal.Round((decimal)goalkk, 0);
                goalYPos = height - margin - ((float)goalkk / (float)maxValue * maxHeight);
                goalHYPos = height - margin - ((float)(goalkk + goalDelta) / (float)maxValue * maxHeight);
                goalLYPos = height - margin - ((float)(goalkk - goalDelta) / (float)maxValue * maxHeight);
            }


            // Создание битмапа 
            using var image = new Image<Rgba32>(width, height);

            // Настройки кисти и пера
            var pen = Pens.Solid(Color.Black, 2);
            var dashedPen = Pens.Dash(Color.Cyan, 1);// { DashPattern = new float[] { 5, 5 } };

            // Загружаем системный шрифт
            var fontCollection = new FontCollection();

            // Change FontFamily name as per your system installed fonts
            //FontFamily fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == "Arial"); //?? SystemFonts.Collection.AddSystemFontCollection().Families.First();
            //Font font = fontFamily.CreateFont(11, SixLabors.Fonts.FontStyle.Regular);

            //var fontPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "TTF","Roboto-Black.ttf");
            var fontPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "TTF", "Roboto-Regular.ttf"); FontFamily fontFamily = fontCollection.Add(fontPath);
            Font font = fontFamily.CreateFont(19, FontStyle.Italic);
            Font smallfont = fontFamily.CreateFont(10, FontStyle.Italic);
            Font medfont = fontFamily.CreateFont(13, FontStyle.Italic);
            image.Mutate(ctx =>
            {
                // Заполнение фона белым цветом
                ctx.Fill(Color.White);

                // Нарисуем оси

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
                ctx.DrawText("ккал", medfont, Color.Black, new PointF(margin - textFromAxe + 17, margin));

                
                // Отрисовка колонок
                for (int i = 0; i < values.Length; i++)
                {
                    //определение цвета
                    var barCollor = Color.LightSeaGreen;
                    float barHeight = (float)(values[i] / maxValue * maxHeight);
                    if (goalkk != null && goalkk > 0 && goalkk <= maxValue)
                    {
                        if (height - margin - barHeight < goalHYPos)
                            barCollor = Color.OrangeRed;
                        if (height - margin - barHeight > goalLYPos)
                            barCollor = Color.LightBlue;
                    }

                    // Определение высоты баров
                    var barRectangle = new RectangularPolygon(
                                      x: margin + barSpace + i * barWidth,
                                      y: height - margin - barHeight,
                                      width: barWidth - barSpace,
                                      height: barHeight);

                    ctx.Fill(barCollor, barRectangle);
                    if(values[i] >200m)
                    if (barCollor == Color.LightBlue)
                        ctx.DrawText(values[i].ToString(), medfont, Color.Black, new PointF(margin + i * barWidth + (barWidth - barSpace) / 4 + 15, height - margin - barHeight + 5));
                   else
                        ctx.DrawText(values[i].ToString(), medfont, Color.White, new PointF(margin + i * barWidth + (barWidth - barSpace) / 4 + 15, height - margin - barHeight + 5));

                    ctx.DrawText(labels[i], font, Color.Black, new PointF(margin + i * barWidth + (barWidth - barSpace) / 4, height - margin + 5));
                }

                const int labelPadding = -5;

                if (goalkk != null && goalkk > 0)
                {
                    ctx.DrawLine(Pens.Solid(Color.LightSeaGreen, 1), new PointF(margin, goalYPos), new PointF(width - margin, goalYPos));
                    ctx.DrawText("норма", smallfont, Color.LightSeaGreen, new PointF(margin - textFromAxe - 40, goalYPos - labelPadding + 5));
                    ctx.DrawText(((decimal)goalkk).ToString("#"), font, Color.LightSeaGreen, new PointF(margin - textFromAxe, goalYPos - labelPadding));

                    float dashLength = 10;
                    float spaceLength = 10;
                    for (float x = margin; x < width - margin; x += dashLength + spaceLength)
                    {
                        float xEnd = Math.Min(x + dashLength, width - margin);
                        ctx.DrawLine(Pens.Solid(Color.Cyan, 1), new PointF(x, goalLYPos), new PointF(xEnd, goalLYPos));
                    }
                    // ctx.DrawLine(dashedPen, new PointF(margin, goalLYPos), new PointF(width - margin, goalLYPos));
                    ctx.DrawText("недостаточно", smallfont, Color.Black, new PointF(margin - textFromAxe - 70, goalLYPos - labelPadding));
                    ctx.DrawText("на 10%", smallfont, Color.Black, new PointF(margin - textFromAxe - 40, goalLYPos - labelPadding + 10));
                    ctx.DrawText(((decimal)goalkk - goalDelta).ToString("#"), font, Color.Black, new PointF(margin - textFromAxe, goalLYPos - labelPadding));

                    for (float x = margin; x < width - margin; x += dashLength + spaceLength)
                    {
                        float xEnd = Math.Min(x + dashLength, width - margin);
                        ctx.DrawLine(Pens.Solid(Color.Cyan, 1), new PointF(x, goalHYPos), new PointF(xEnd, goalHYPos));
                    }
                    //ctx.DrawLine(dashedPen, new PointF(margin, goalHYPos), new PointF(width - margin, goalHYPos));
                    ctx.DrawText("превышение", smallfont, Color.Black, new PointF(margin - textFromAxe - 70, goalHYPos - labelPadding));
                    ctx.DrawText("на 10%", smallfont, Color.Black, new PointF(margin - textFromAxe - 40, goalHYPos - labelPadding + 10));
                    ctx.DrawText(((decimal)goalkk + goalDelta).ToString("#"), font, Color.Black, new PointF(margin - textFromAxe, goalHYPos - labelPadding));
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

