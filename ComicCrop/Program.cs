using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ComicCrop
{
    class Program
    {
        public static string TarDir { get; set; }
        public static string CurDir { get; set; }

        static void Main(string[] args)
        {
            TarDir = @"E:\Comic";
            CurDir = @"D:\Comic";
            FindFile(CurDir);
            return;

            TaskFactory tf = new TaskFactory();
            string[] filePaths = Directory.GetFiles(@"D:\新建文件夹");
            Task[] tasks = new Task[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                tasks[i] = tf.StartNew(Convert, filePaths[i]);
            }

            Task.WaitAll(tasks);

            Console.WriteLine("Scuess!");
        }

        public static void Convert(object state)
        {
            string filePath = state.ToString();
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExt = Path.GetExtension(filePath);
            fileExt = ".jpg";
            Console.WriteLine(fileName);
            using (var image = Image.Load(filePath))
            {
                if (image.Width <= image.Height)
                {
                    image.Save($@"D:\output\{fileName}{fileExt}");
                    return;
                }
                Rectangle rectangleLeft = new Rectangle(image.Width / 2, 0, image.Width / 2, image.Height);
                Rectangle rectangleRight = new Rectangle(0, 0, image.Width / 2, image.Height);
                using (var imgLeft = image.Clone(x => x
                       .Crop(rectangleLeft)))
                {
                    imgLeft.Save($@"D:\output\{fileName}_1{fileExt}");
                };
                using (var imgRight = image.Clone(x => x
                       .Crop(rectangleRight)))
                {
                    imgRight.Save($@"D:\output\{fileName}_2{fileExt}");
                };
            }
        }

        public static void FindFile(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
            dirInfos.ToList().ForEach(s =>
            {
                string dir1 = s.FullName.Replace(CurDir, "").TrimStart('\\');
                string dir2 = Path.Combine(TarDir, dir1);
                DirectoryInfo info2 = new DirectoryInfo(dir2);
                if (!info2.Exists)
                {
                    info2.Create();
                }
            });

            if (dirInfos.Length == 0)
            {
                return;
            }
            dirInfos.ToList().ForEach(s => FindFile(s.FullName));
        }
    }
}
