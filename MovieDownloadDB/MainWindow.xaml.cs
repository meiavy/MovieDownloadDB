using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HgCo.WindowsLive.SkyDrive;

namespace MovieDownloadDB
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private MovieDB _db = null;
        private List<String> _dirs = new List<string>();
        private List<String> _errorDirs = new List<string>();

        private void Window_Activated_1(object sender, EventArgs e)
        {

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            _db = MovieDB.Instance();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if("".Equals(textBoxMovieName.Text))
            {
                return;
            }
            List<MovieFile> movies=_db.Find(textBoxMovieName.Text);
            dataGridResult.ItemsSource = movies;
            textBlockCount.Text ="返回"+ movies.Count.ToString()+"条结果！";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if("".Equals(textBoxSearchPath.Text))
            {
                return;
            }

            listBoxError.ItemsSource = null;
            _errorDirs.Clear();

            _dirs.Clear();

            GetDirList(textBoxSearchPath.Text);

            List<MovieFile> movies = new List<MovieFile>();

            foreach (string dir in _dirs)
            {
                string[] files = GetFiles(dir);
                if (files == null)
                {
                    continue;
                }

                foreach (string file in files)
                {
                    long size = new FileInfo(file).Length;
                    if (size > 3 * 1024 * 1024)
                    {
                        string name = System.IO.Path.GetFileName(file);
                        string time = File.GetCreationTime(file).ToString("yyyy-MM-dd HH:mm:ss");
                        MovieFile movie = new MovieFile(name, time, size, file);
                        movies.Add(movie);
                    }
                }

            }

            _db.Add(movies);

            //listBoxError

            listBoxError.ItemsSource = _errorDirs;

        }

        private string[] GetFiles(String root)
        {
            try
            {
                return Directory.GetFiles(root);
            }
            catch
            {
                _errorDirs.Add(root);
            }
            return null;
        }

        private void GetDirList(String root)
        {
            _dirs.Add(root);
            string[] subDirs = null;
            try
            {
                subDirs=Directory.GetDirectories(root);
                if (subDirs == null)
                {
                    return ;
                }

                foreach (string dir in subDirs)
                {
                    GetDirList(dir);
                }

            }
            catch
            {
                _errorDirs.Add(root);
            }            
        }



        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result= MessageBox.Show(this,"你确定要清空数据库吗？", "清空数据库", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                _db.Clear();
            }
           
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            List<MovieFile> movies = _db.FindTheSameMovies();
            dataGridResult.ItemsSource = movies;
            textBlockCount.Text = "返回" + movies.Count.ToString() + "条结果！";
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            System.Collections.IList items = dataGridResult.SelectedItems;
            if (items == null)
            {
                return;
            }

            foreach (MovieFile movie in items)
            {
                _db.Delete(movie);
            }

           

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            SkyDriveWebClient client = new SkyDriveWebClient();
            client.LogOn("zouyongyuan@live.com", "zyy52523!");
            WebFolderInfo info=new WebFolderInfo();
            info.Name="aaa";
            Boolean b= client.IsWebFolderExists(info);
        }
    }
}
