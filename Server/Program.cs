using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Server
{
    class Start
    {
        static async Task Main(string[] args)
        {
            await Database.Database.DatabaseInit(); // Init database
            await Server.Server.StartServerAsync(); // start server
        }
    }
}


//// Загрузка изображения
//Bitmap originalImage = new Bitmap("1.jpg");

//// Новые размеры для аватара (например, 200x200)
//int diameter = 45;

//// Создание нового изображения с прозрачным фоном
//Bitmap resizedImage = new Bitmap(diameter, diameter, PixelFormat.Format32bppArgb);

//// Использование графики для рисования круглой формы
//using (Graphics g = Graphics.FromImage(resizedImage))
//{
//    g.Clear(Color.Transparent); // Заполнение фона прозрачным цветом
//    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
//    g.SmoothingMode = SmoothingMode.AntiAlias;

//    // Создание круглой области
//    GraphicsPath path = new GraphicsPath();
//    path.AddEllipse(0, 0, diameter, diameter);
//    Region region = new Region(path);
//    g.SetClip(region, CombineMode.Replace);

//    // Рисование изображения в круглой области
//    g.DrawImage(originalImage, 0, 0, diameter, diameter);
//}

//// Сохранение нового изображения в формате PNG с прозрачным фоном
//resizedImage.Save("avatar.png", ImageFormat.Png);

//// Освобождение ресурсов
//originalImage.Dispose();
//resizedImage.Dispose();

