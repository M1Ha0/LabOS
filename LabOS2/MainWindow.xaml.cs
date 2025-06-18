using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Threading.Tasks;
using Path = System.IO.Path;
using Microsoft.Win32;

namespace LabOS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _totalFiles;
        private int _copiedFiles;
        private readonly ConcurrentQueue<string> _filesQueue = new ConcurrentQueue<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Выберите папку"
            };

            if (dialog.ShowDialog() == true)
            {
                SourcePathTextBox.Text = Path.GetDirectoryName(dialog.FileName);
            }
        }

        private void SelectDestination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Выберите папку"
            };

            if (dialog.ShowDialog() == true)
            {
                DestinationPathTextBox.Text = Path.GetDirectoryName(dialog.FileName);
            }
        }

        private async void StartCopy_Click(object sender, RoutedEventArgs e)
        {
            string sourceDir = SourcePathTextBox.Text;
            string destDir = DestinationPathTextBox.Text;

            if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(destDir))
            {
                StatusTextBlock.Text = "Укажите исходную и целевую директории";
                return;
            }

            if (!Directory.Exists(sourceDir))
            {
                StatusTextBlock.Text = "Исходная директория не существует";
                return;
            }

            if (!int.TryParse(ThreadCountTextBox.Text, out int threadCount) || threadCount < 1 || threadCount > 16)
            {
                StatusTextBlock.Text = "Количество потоков должно быть от 1 до 16";
                return;
            }
            Directory.CreateDirectory(destDir);
            var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            _totalFiles = files.Length;
            _copiedFiles = 0;

            StatusTextBlock.Text = $"Найдено {_totalFiles} файлов для копирования";
            ProgressBar.Value = 0;
            ProgressTextBlock.Text = "0%";

            foreach (var file in files)
            {
                _filesQueue.Enqueue(file);
            }
            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() => CopyFilesWorker(sourceDir, destDir));
            }

            await Task.WhenAll(tasks);
            StatusTextBlock.Text = "Копирование завершено";
        }

        private void CopyFilesWorker(string sourceDir, string destDir)
        {
            while (_filesQueue.TryDequeue(out string filePath))
            {
                try
                {
                    string relativePath = Path.GetRelativePath(sourceDir, filePath);
                    string destPath = Path.Combine(destDir, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

                    File.Copy(filePath, destPath, true);
                    Dispatcher.Invoke(() =>
                    {
                        _copiedFiles++;
                        double progress = (double)_copiedFiles / _totalFiles * 100;
                        ProgressBar.Value = progress;
                        ProgressTextBlock.Text = $"{progress:0}%";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                        StatusTextBlock.Text = $"Ошибка при копировании {filePath}: {ex.Message}");
                }
            }
        }
    }
}