using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFFolderBrowser;

namespace DiskAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<FileInfo> GetFilesInDirectory(string directory)
        {
            var files = new List<FileInfo>();
            try
            {
                var directories = Directory.GetDirectories(directory);
                try
                {
                    var di = new DirectoryInfo(directory);
                    files.AddRange(di.GetFiles("*"));
                }
                catch
                {
                }
                foreach (var dir in directories)
                {
                    files.AddRange(GetFilesInDirectory(Path.Combine(directory, dir)));
                }
            }
            catch 
            {
            }
           
            return files;
        }

        private async void StartClick(object sender, RoutedEventArgs e)
        {
            var fbd = new WPFFolderBrowserDialog();
            if (fbd.ShowDialog() != true)
                return;
            var selectedPath = fbd.FileName;
            Int64 minSize;
            if (!Int64.TryParse(MinSizeBox.Text, out minSize))
                return;
            List<FileInfo> files = null;
            await Task.Factory.StartNew( () => 
              files = GetFilesInDirectory(selectedPath)
                .Where(f => f.Length >= minSize)
                .OrderByDescending(f => f.Length)
                .ToList());
            var totalSize = files.Sum(f => f.Length);
            TotalFilesText.Text = $"# Files: {files.Count}";
            LengthFilesText.Text = $"({totalSize:N0} bytes)";
            FilesList.ItemsSource = files;
            var extensions = files.GroupBy(f => f.Extension)
                .Select(g => new {Extension = g.Key, Quantity = g.Count(), Size = g.Sum(f => f.Length)})
                .OrderByDescending(t => t.Size).ToList();
            ExtList.ItemsSource = extensions;
            ExtSeries.ItemsSource = extensions;
            var tmp = 0.0;
            var abcData = files.Select(f =>
            {
                tmp += f.Length;
                return new {f.Name, Percent = tmp/totalSize*100};
            }).ToList();
            AbcList.ItemsSource = abcData;
            AbcSeries.ItemsSource = abcData.OrderBy(d => d.Percent).Select((d,i) => new {Item = i, d.Percent});
        }
    }
}
