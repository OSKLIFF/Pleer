using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace pleer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Logic logic = new Logic();
        public string FilePath { get { return FilePath_TB.Text; } }
        public string GetFilePath(string filepath)
        {
            return FilePath;
        }
        bool pause = false;
        bool auto;
        string s;
        string nf;
        static string path;
        string extension;
        string a;
        int i;
        int mp3;
        int mp4;

        private void OpenFilePath_BT_Click(object sender, RoutedEventArgs e)
        {
            i = 0;
            mp3 = 0;
            mp4 = 0;
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                s = FBD.SelectedPath;
                logic.FolderPath(s);
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                logic.ReturnCollection_mp3(s);
                logic.ReturnCollection_mp4(s);
                dlg.Filter = "Файлы mp3|*.mp3|Файлы mp4|*.mp4";
                if (dlg.ShowDialog() == true)
                {
                    FilePath_TB.Text = dlg.FileName;
                    GetFilePath(FilePath_TB.Text);
                    path = FilePath_TB.Text;
                    a = System.IO.Path.GetFileName(path);
                    extension = System.IO.Path.GetExtension(path);
                    switch (extension)
                    {
                        case ".mp3":
                            i = logic.collection_mp3.IndexOf(a);
                            mp3 = i + 1;
                            if (mp3 < logic.collection_mp3.Count)
                            {
                                Next_BT.IsEnabled = true;
                            }
                            if (mp3 >= 2)
                            {
                                Back_BT.IsEnabled = true;
                            }
                            break;
                        case ".mp4":
                            i = logic.collection_mp4.IndexOf(a);
                            mp4 = i + 1;
                            if (mp4 < logic.collection_mp4.Count)
                            {
                                Next_BT.IsEnabled = true;
                            }
                            if (mp4 >= 2)
                            {
                                Back_BT.IsEnabled = true;
                            }
                            break;
                    }
                    bool isExist = logic.isExist(FilePath_TB.Text);
                    pause = true;
                    if (!isExist)
                    {
                        System.Windows.MessageBox.Show("Выбранный файл не сущесвует!");
                        return;
                    }
                    ShowContent(FilePath);
                }
                if (FilePath_TB.Text == null)
                {
                    pause = false;
                    System.Windows.MessageBox.Show("Вы не выбрали файл!");
                }
            }
            else System.Windows.MessageBox.Show("Такой папки не существует!");
        }

        //static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        //CancellationToken token = cancelTokenSource.Token;
        async void ShowContent(string filepath)
        {
            MediaContentViewer_ME.Source = new Uri(filepath);
            await Task.Run(() => {
                Dispatcher.Invoke((Action)(() =>
                {
                    MediaContentViewer_ME.Play();
                }));
            }/*, token*/);
        }

        #region[Взаимодействие с контентом]
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            MediaContentViewer_ME.Volume = (double)volumeSlider.Value;
        }

        DispatcherTimer _timer = new DispatcherTimer();
        private void MediaContentViewer_ME_MediaOpened(object sender, RoutedEventArgs e) //когда файл загружен
        {
            timelineSlider.Maximum = MediaContentViewer_ME.NaturalDuration.TimeSpan.TotalSeconds; //устанавливаем максимальное значение для слайдера отвечающего за длинну проигрываемого ролика
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += new EventHandler(sinchronyzeSliderAndVideoSession);
            _timer.Start();
            auto = false;
        }
        private void sinchronyzeSliderAndVideoSession(object sender, EventArgs e)
        {
            timelineSlider.Value = MediaContentViewer_ME.Position.TotalSeconds;
            Timer_view.Text = Convert.ToString(new TimeSpan(0, 0, 0, Convert.ToInt16(Math.Round(timelineSlider.Value))));
        }

        private void MediaContentViewer_ME_MediaEnded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            if (AutoNextFile_CB.IsChecked == true)
            {
                auto = true;
                AutoNextFile_CB_Checked(sender, e);
            }
        }

        private void timelineSlider_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _timer.Stop();
            MediaContentViewer_ME.Pause();
        }

        private void timelineSlider_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e) //реализум возможность перемотки видео/аудио трека перетаскиванием ползунка слайдера. Срабатывает на момент отпускания клавиши мышки
        {
            TimeSpan time = new TimeSpan(0, 0, 0, Convert.ToInt32(Math.Round(timelineSlider.Value))); //отлавливаем позицию на которую нужно перемотать трек
            if (Pause_BT.Name == "Pause_BT")
            {
                _timer.Start();
                MediaContentViewer_ME.Play();
            }
            if (Pause_BT.Name == "Continue_BT")
            {
                Timer_view.Text = Convert.ToString(new TimeSpan(0, 0, 0, Convert.ToInt16(Math.Round(timelineSlider.Value))));
            }
            MediaContentViewer_ME.Position = time; //устанавливаем новую позицию для трека
        }

        #endregion
        private void Pause_BT_Click(object sender, RoutedEventArgs e)
        {
            if (pause == true)
            {
                switch (Pause_BT.Name)
                {
                    case "Pause_BT":
                        Pause_BT.Name = "Continue_BT";
                        Pause_BT.Content = "Продолжить";
                        _timer.Stop();
                        MediaContentViewer_ME.Pause();
                        break;
                    case "Continue_BT":
                        Pause_BT.Name = "Pause_BT";
                        Pause_BT.Content = "Пауза";
                        _timer.Start();
                        MediaContentViewer_ME.Play();
                        break;
                }
            }
            else System.Windows.MessageBox.Show("Файл не выбран!");
        }

        private void Next_BT_Click(object sender, RoutedEventArgs e)
        {
            if (FilePath_TB.Text != "")
            {
                if (mp3 < logic.collection_mp3.Count || mp4 < logic.collection_mp4.Count)
                {
                    switch (extension)
                    {
                        case ".mp3":
                            if (mp3 < logic.collection_mp3.Count)
                            {
                                i = i + 1;
                                nf = logic.collection_mp3.ElementAt(i);
                                string NextPath = Convert.ToString(logic.FolderPath(s) + "\\" + nf);
                                MediaContentViewer_ME.Close();
                                FilePath_TB.Text = NextPath;
                                ShowContent(NextPath);
                                Back_BT.IsEnabled = true;
                                mp3++;
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Файлы закончились ;(");
                                Next_BT.IsEnabled = false;
                            }
                            break;

                        case ".mp4":
                            if (mp4 < logic.collection_mp4.Count)
                            {
                                i = i + 1;
                                nf = logic.collection_mp4.ElementAt(i);
                                string NextPath = Convert.ToString(logic.FolderPath(s) + "\\" + nf);
                                MediaContentViewer_ME.Close();
                                FilePath_TB.Text = NextPath;
                                ShowContent(NextPath);
                                Back_BT.IsEnabled = true;
                                mp4++;
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Файлы закончились ;(");
                                Next_BT.IsEnabled = false;
                            }
                            break;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Файлов нет ;(");
                    Next_BT.IsEnabled = false;
                }
            }
            else System.Windows.MessageBox.Show("Вы не выбрали Файл/Директорию");
        }

        private void Back_BT_Click(object sender, RoutedEventArgs e)
        {
            if (FilePath_TB.Text != "")
            {
                if (mp3 >= 2 || mp4 >= 2)
                {
                    switch (extension)
                    {
                        case ".mp3":
                            if (mp3 > 1)
                            {
                                mp3--;
                                i = i - 1;
                                nf = logic.collection_mp3.ElementAt(i);
                                string NextPath = Convert.ToString(logic.FolderPath(s) + "\\" + nf);
                                MediaContentViewer_ME.Close();
                                FilePath_TB.Text = NextPath;
                                ShowContent(NextPath);
                                Next_BT.IsEnabled = true;
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Вы на первом файле !");
                                Back_BT.IsEnabled = false;
                            }
                            break;

                        case ".mp4":
                            if (mp4 > 1)
                            {
                                mp4--;
                                i = i - 1;
                                nf = logic.collection_mp4.ElementAt(i);
                                string NextPathmp4 = Convert.ToString(logic.FolderPath(s) + "\\" + nf);
                                MediaContentViewer_ME.Close();
                                FilePath_TB.Text = NextPathmp4;
                                ShowContent(NextPathmp4);
                                Next_BT.IsEnabled = true;
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Вы на первом файле !");
                                Back_BT.IsEnabled = false;
                            }
                            break;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Вы на первом файле !");
                    Back_BT.IsEnabled = false;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Вы не выбрали Файл/Директорию");
            }
        }

        private void AutoNextFile_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoNextFile_CB.IsChecked == true)
            {
                if (auto == true)
                {
                    switch (extension)
                    {
                        case ".mp3":
                            if (mp3 < logic.collection_mp3.Count)
                            {
                                i = i + 1;
                                nf = logic.collection_mp3.ElementAt(i);
                                string NextPath = Convert.ToString(logic.FolderPath(s) + "\\" + nf);
                                MediaContentViewer_ME.Close();
                                FilePath_TB.Text = NextPath;
                                ShowContent(NextPath);
                                Back_BT.IsEnabled = true;
                                mp3++;
                            }
                            else System.Windows.MessageBox.Show("Файлы закончились :)");
                            break;
                        case ".mp4":
                            if (mp4 < logic.collection_mp4.Count)
                            {
                                i = i + 1;
                                nf = logic.collection_mp4.ElementAt(i);
                                string NextPath = Convert.ToString(logic.FolderPath(s) + "\\" + nf);
                                MediaContentViewer_ME.Close();
                                FilePath_TB.Text = NextPath;
                                ShowContent(NextPath);
                                Back_BT.IsEnabled = true;
                                mp4++;
                            }
                            else System.Windows.MessageBox.Show("Файлы закончились :)");
                            break;
                    }
                }
            }
        }
    }
}
