using System;
using System.IO;
using System.Windows;
using System.Net.Http;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Weather
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        string url;

        public MainWindow()
        {
            InitializeComponent();
            this.url = GetUrl();
            GetWeather(url);

            timer = new DispatcherTimer();
            UpdateTimer();

            SystemEvents.SessionEnded += this.SystemEvents_Power;
            NetworkChange.NetworkAvailabilityChanged += this.NetworkChange_NetworkAvailabilityChanged;
        }

        private void SystemEvents_Power(object sender, SessionEndedEventArgs e)
        {
            Dispatcher.Invoke(() => GetWeather(url));
        }

        private void NetworkChange_NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Dispatcher.Invoke(() => GetWeather(url));
            }
        }

        // Загрузка ссылки на город из файла
        private string GetUrl()
        {
            string url = "https://weather.com/ru-RU/weather/today/l/464c57daded904c43a47deedb004aa696e9e6cc12f5907e7f4c884ff8cfd9157";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Data.txt");

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string? line = string.Empty;
                    if ((line = reader.ReadLine()) != null)
                        url = line;
                }
            }
            catch
            {
                return url;
            }

            return url;
        }

        // Сохранение ссылки на город в файл
        private void SaveUrl()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Data.txt");

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    writer.WriteLine(url);
                }
            }
            catch
            {
                return;
            }
        }

        // Обновление погоды в заданном интервале
        private void Timer_Tick(object? sender, EventArgs e)
        {
            GetWeather(url);
        }

        // Установка таймера
        private void UpdateTimer()
        {
            int updateTime = 20;

            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, updateTime, 0);
            timer.Start();
        }

        // Получение данных со страницы погоды
        private async void GetWeather(string url)
        {
            ErrorInfo.Visibility = Visibility.Hidden;
            LoadInfo.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                string doc = GetHtml(url);
                WeatherData data = new WeatherData(doc);
                Dispatcher.Invoke(() => ShowWeather(data));
            });
        }

        // Сопоставление длины текста и размеров поля вывода Label
        private bool IsTextTrimmed(string text, System.Windows.Controls.Label label)
        {
            var typeface = new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch);

            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture,
            label.FlowDirection, typeface, label.FontSize, label.Foreground,
            VisualTreeHelper.GetDpi(label).PixelsPerDip);

            return formattedText.Width > label.Width;
        }

        // Вывод данных о погоде на экран
        private void ShowWeather(WeatherData data)
        {
            try
            {
                WeatherInfo.Visibility = Visibility.Visible;
                WeatherInfo.Background = new ImageBrush(new BitmapImage(new Uri(data.ImageSourse)));
                LoadInfo.Visibility = Visibility.Hidden;
            }
            catch
            {
                WeatherInfo.Visibility = Visibility.Hidden;
                LoadInfo.Visibility = Visibility.Hidden;
                ErrorInfo.Visibility = Visibility.Visible;
                return;
            }

            TempLabel.Content = data.Temperature;
            TempLabel.Foreground = new SolidColorBrush(data.FontColor);

            StatusTB.Text = data.WeatherStatus;            
            StatusLabel.ToolTip = IsTextTrimmed(data.WeatherStatus, StatusLabel) ? data.WeatherStatus : null;
            StatusLabel.Foreground = new SolidColorBrush(data.FontColor);           

            CityTB.Text = data.City;
            CityLabel.ToolTip = IsTextTrimmed(data.City, CityLabel) ? data.City : null;
            CityLabel.Foreground = new SolidColorBrush(data.FontColor);
        }

        // Получение страницы с данными о погоде
        private string GetHtml(string url)
        {
            string pageContent = string.Empty;            
            
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");
            try
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        pageContent = content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch
            {
                return pageContent;
            }

            return pageContent;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
            MainGrid.Visibility = Visibility.Hidden;
        }

        private void MoveWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ProgramWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            Buttons.Visibility = Visibility.Visible;
        }

        private void ProgramWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            Buttons.Visibility = Visibility.Hidden;
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsInfo.Visibility = Visibility.Visible;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsInfo.Visibility = Visibility.Hidden;
            WeatherInfo.Visibility = Visibility.Hidden;

            url = CityLinkTB.Text;
            CityLinkTB.Text = string.Empty;
            SaveUrl();
            GetWeather(url);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CityLinkTB.Text = string.Empty;
            SettingsInfo.Visibility = Visibility.Hidden;
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {            
            GetWeather(url);
        }
    }
}