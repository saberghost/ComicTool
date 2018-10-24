using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Microsoft.Extensions.Configuration;

namespace ComicCrop
{
    class Program
    {
        public static AppSettings AppSettings { get; set; } = new AppSettings();

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true).Build();
            builder.Bind(AppSettings);
            Console.WriteLine();


            Excute(AppSettings.SrcPath);

            Console.WriteLine("Scuess!");
        }

        public static void CropPictureTask(string SrcDirPath, string DstDirPath)
        {
            TaskFactory tf = new TaskFactory();
            string[] filePaths = Directory.GetFiles(SrcDirPath);
            Task[] tasks = new Task[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                Tuple<string, string> tuple = new Tuple<string, string>(filePaths[i], DstDirPath);
                tasks[i] = tf.StartNew(CropPicture, tuple);
            }

            Task.WaitAll(tasks);
        }

        public static void CropPicture(object state)
        {
            Tuple<string, string> tuple = state as Tuple<string, string>;
            string DstDirPath = tuple.Item2;
            string filePath = tuple.Item1;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExt = Path.GetExtension(filePath);
            fileExt = ".jpg";
            Console.WriteLine(fileName);
            using (var image = Image.Load(filePath))
            {
                if (image.Width <= image.Height)
                {
                    image.Save(Path.Combine(DstDirPath, $"{fileName}{fileExt}"));
                    return;
                }
                Rectangle rectangleLeft = new Rectangle(image.Width / 2, 0, image.Width / 2, image.Height);
                Rectangle rectangleRight = new Rectangle(0, 0, image.Width / 2, image.Height);
                using (var imgLeft = image.Clone(x => x
                       .Crop(rectangleLeft)))
                {
                    imgLeft.Save(Path.Combine(DstDirPath, $"{fileName}_1{fileExt}"));
                };
                using (var imgRight = image.Clone(x => x
                       .Crop(rectangleRight)))
                {
                    imgRight.Save(Path.Combine(DstDirPath, $"{fileName}_2{fileExt}"));
                };
            }
        }

        public static void Excute(string DirPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(DirPath);
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
            dirInfos.ToList().ForEach(s =>
            {
                string tmpDirPath = s.FullName.Replace(AppSettings.SrcPath, "").TrimStart('\\');
                string DstDirPath = Path.Combine(AppSettings.DstPath, tmpDirPath);
                DirectoryInfo DstDirInfo = new DirectoryInfo(DstDirPath);
                if (!DstDirInfo.Exists)
                {
                    DstDirInfo.Create();
                }
                CropPictureTask(s.FullName, DstDirPath);
            });

            dirInfos.ToList().ForEach(s => Excute(s.FullName));
        }
    }
}
