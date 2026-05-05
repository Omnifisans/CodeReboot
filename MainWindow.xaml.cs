using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
namespace CodeRebootWPF
{
    public partial class MainWindow : Window
    {
        // Игрок
        private Image player;
        private double playerX = 200;
        private double playerY = 350;
        private double playerSpeed = 2;

        // Мир
        private Canvas worldCanvas;
        private List<Rect> obstacles = new List<Rect>();
        private List<GameObject> objects = new List<GameObject>();
        private Rectangle darkOverlay;
        private GameObject terminalObject;
        private GameObject doorObject;

        public bool IsTerminalOpen { get; private set; } = false;

        // Состояние уровня
        private int currentLevel = 1;
        private bool exitDoorLocked = true;
        private int powerLevel = 0;
        private bool generatorActive = false;
        private int doorCode = 0;
        private string activationCode = " ";
        private int generatorToggles = 0;
        private bool generatorBroken = false;
        private bool hasPiece1 = false;
        private bool hasPiece2 = false;
        private bool doorSpawned = false;

        private bool escPressed = false;
        private DateTime escPressStartTime;

        private Image roomBg;
        private TranslateTransform roomTransform;

        // Анимация 
        private Dictionary<string, BitmapImage> playerSprites = new Dictionary<string, BitmapImage>();
        private int animCounter = 0;
        private const int AnimSpeed = 40;
        private string currentDirection = "down";

        // Путь к сохранению
        private static readonly string SavePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeRebootWPF", "save.xml");

        public MainWindow()
        {
            InitializeComponent();

            // Новая игра
            currentLevel = 1;
            exitDoorLocked = true;
            powerLevel = 0;
            generatorActive = false;
            doorCode = 0;
            activationCode = " ";
            generatorToggles = 0;
            generatorBroken = false;
            hasPiece1 = false;
            hasPiece2 = false;
            doorSpawned = false;
            escPressed = false;
            IsTerminalOpen = false;
            playerX = 200;
            playerY = 350;

            // Настройка окна
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.NoResize;
            this.Left = 0; this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            // Сохраняем при закрытии окна
            this.Closing += (s, e) => SaveGame();

            // Фон комнаты
            roomBg = new Image();
            roomBg.Source = new BitmapImage(new Uri("pack://application:,,,/images/room.png"));
            roomBg.Stretch = Stretch.None;
            GameCanvas.Children.Add(roomBg);
            Canvas.SetZIndex(roomBg, -2);

            worldCanvas = new Canvas { Background = Brushes.Transparent };
            GameCanvas.Children.Add(worldCanvas);
            roomTransform = new TranslateTransform();
            worldCanvas.RenderTransform = roomTransform;

            darkOverlay = new Rectangle { Width = 800, Height = 600, Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) };
            darkOverlay.Visibility = Visibility.Collapsed;
            worldCanvas.Children.Add(darkOverlay);
            Canvas.SetZIndex(darkOverlay, 100);

            // Инициализация спрайтов
            string Pack(string name) => $"pack://application:,,,/images/{name}";
            playerSprites["down_stay"] = new BitmapImage(new Uri(Pack("robot.png")));
            playerSprites["down_walk1"] = new BitmapImage(new Uri(Pack("robot_walk1.png")));
            playerSprites["down_walk2"] = new BitmapImage(new Uri(Pack("robot_walk2.png")));
            playerSprites["up_stay"] = new BitmapImage(new Uri(Pack("robot_back_stay.png")));
            playerSprites["up_walk1"] = new BitmapImage(new Uri(Pack("robot_back_walk1.png")));
            playerSprites["up_walk2"] = new BitmapImage(new Uri(Pack("robot_back_walk2.png")));
            playerSprites["left_stay"] = new BitmapImage(new Uri(Pack("robot_left_stay.png")));
            playerSprites["left_walk1"] = new BitmapImage(new Uri(Pack("robot_left_walk1.png")));
            playerSprites["left_walk2"] = new BitmapImage(new Uri(Pack("robot_left_walk2.png")));
            playerSprites["right_stay"] = new BitmapImage(new Uri(Pack("robot_right_stay.png")));
            playerSprites["right_walk1"] = new BitmapImage(new Uri(Pack("robot_right_walk1.png")));
            playerSprites["right_walk2"] = new BitmapImage(new Uri(Pack("robot_right_walk2.png")));

            // Загружаем уровень 1 (без LoadGame)
            LoadLevel(currentLevel);
            CompositionTarget.Rendering += GameLoop;

            GameCanvas.Focusable = true;
            GameCanvas.Focus();

            this.SizeChanged += OnSizeChanged;
            CenterRoom();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => CenterRoom();

        private void CenterRoom()
        {
            if (roomBg == null) return;
            double canvasWidth = GameCanvas.ActualWidth;
            double canvasHeight = GameCanvas.ActualHeight;
            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            Canvas.SetLeft(roomBg, (canvasWidth - roomBg.Source.Width) / 2);
            Canvas.SetTop(roomBg, (canvasHeight - roomBg.Source.Height) / 2);

            roomTransform.X = (canvasWidth - 800) / 2;
            roomTransform.Y = (canvasHeight - 600) / 2;
        }

        // Сохранение
        private void SaveGame()
        {
            try
            {
                string dir = System.IO.Path.GetDirectoryName(SavePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var data = new SaveData
                {
                    CurrentLevel = currentLevel,
                    PlayerX = playerX,
                    PlayerY = playerY
                };
                var serializer = new XmlSerializer(typeof(SaveData));
                using (var writer = new StreamWriter(SavePath))
                    serializer.Serialize(writer, data);
            }
            catch { }
        }

        // Загрузка (публичный метод для кнопки "Продолжить")
        public void LoadGame()
        {
            if (!File.Exists(SavePath)) return;
            try
            {
                var serializer = new XmlSerializer(typeof(SaveData));
                using (var reader = new StreamReader(SavePath))
                {
                    var data = (SaveData)serializer.Deserialize(reader);
                    if (data != null)
                    {
                        currentLevel = data.CurrentLevel;
                        playerX = data.PlayerX;
                        playerY = data.PlayerY;
                        // Загружаем уровень с сохранёнными координатами
                        LoadLevel(currentLevel);
                    }
                }
            }
            catch { }
        }
    }

    // Класс сохранения
    public class SaveData
    {
        public int CurrentLevel { get; set; } = 1;
        public double PlayerX { get; set; } = 200;
        public double PlayerY { get; set; } = 350;
    }
}
