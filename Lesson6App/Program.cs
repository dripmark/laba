using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace Lesson6App
{
    class Program
    {
        private static DirectoryInfo _rootDirectory;
        private static string[] _specDirectories = new string[] { "Изображения", "Документы", "Прочее" };
        private static int _imageCount = 0, _documentsCount = 0, _othersCount = 0; 
        static void Main(string[] args)
        {
            Console.WriteLine("Введите путь к диску: ");
            string directoryPath = Console.ReadLine();

            var driveInfo = new DriveInfo(directoryPath);
            Console.WriteLine($"Информация о диске: {driveInfo.VolumeLabel}, всего {driveInfo.TotalSize / 1024 / 1024} МБ, " +
                $"свободно {driveInfo.AvailableFreeSpace / 1024 / 1024} МБ.");

            _rootDirectory = driveInfo.RootDirectory;
            SearchDirectories(_rootDirectory);

            foreach (var directory in _rootDirectory.GetDirectories())
            {
                if (!_specDirectories.Contains(directory.Name))
                    directory.Delete(true);
            }

            var resultText = $"Всего обработано {_imageCount + _documentsCount + _othersCount} файлов" +
                $"Из них {_imageCount} изобрадений, {_documentsCount} документов, {_othersCount} Прочих файлов.";
            Console.WriteLine(resultText);
            File.WriteAllText(_rootDirectory + "\\Инфо.txt", resultText);

            Console.ReadLine();
        }

        private static void SearchDirectories(DirectoryInfo currentDirectory)
        {
            if (!_specDirectories.Contains(currentDirectory.Name))
            {
                FilterFiles(currentDirectory);
                foreach (var childrenDirectory in currentDirectory.GetDirectories())
                {
                    SearchDirectories(childrenDirectory);
                }
            }
        }

        private static void FilterFiles(DirectoryInfo currentDirectory)
        {
            var currentFiles = currentDirectory.GetFiles();

            foreach(var fileInfo in currentFiles)
            {
                if (new string[] {".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp", ".svg" }
                .Contains(fileInfo.Extension.ToLower()))
                {
                    var photoDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[0]}\\");
                    if (!photoDirectory.Exists)
                        photoDirectory.Create();

                    var yearDirectory = new DirectoryInfo(_rootDirectory + $"{fileInfo.LastWriteTime.Date.Year}\\");
                    if (!yearDirectory.Exists)
                        yearDirectory.Create();

                    MoveFile(fileInfo, yearDirectory);
                    _imageCount++;

                }
                else if(new string[] {".doc","docx",".pdf", ".xls", ".xlsx", ".ppt", ".pptx" }
                .Contains(fileInfo.Extension.ToLower()))
                {
                    var documentsDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[1]}\\");
                    if (!documentsDirectory.Exists)
                        documentsDirectory.Create();

                    DirectoryInfo lenghtDirectory = null;
                    if (fileInfo.Length / 1024 / 1024 < 1)
                        lenghtDirectory = new DirectoryInfo(documentsDirectory + "Менее 1 МБ\\");
                    else if (fileInfo.Length / 1024 / 1024 < 10)
                        lenghtDirectory = new DirectoryInfo(documentsDirectory + "Менее 10 МБ\\");
                    else
                        lenghtDirectory = new DirectoryInfo(documentsDirectory + "От 1 до 10 МБ\\");
                    if (!lenghtDirectory.Exists)
                        lenghtDirectory.Create();

                    MoveFile(fileInfo, lenghtDirectory);
                    _documentsCount++;
                }
                else
                {
                    var othersDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[2]}\\");
                    if (!othersDirectory.Exists)
                        othersDirectory.Create();

                    MoveFile(fileInfo, othersDirectory);
                    _othersCount++;
                }
            }
        }
            private static void MoveFile(FileInfo fileInfo, DirectoryInfo directoryInfo)
        {
            var newFileInfo = new FileInfo(directoryInfo + $"\\{fileInfo.Name}");
            while (newFileInfo.Exists)
                newFileInfo = new FileInfo(directoryInfo + $"\\{Path.GetFileNameWithoutExtension(fileInfo.FullName)} (1)" +
                    $"{newFileInfo.Extension}");
            fileInfo.MoveTo(newFileInfo.FullName);
        }
    }
}
