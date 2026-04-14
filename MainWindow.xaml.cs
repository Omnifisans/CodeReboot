using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CodeRebootWPF
{
    public partial class MainWindow : Window
    {
        // ---- Игрок ----
        private Image player;
        private double playerX = 200;
        private double playerY = 350;      // стартовая позиция
        private double playerSpeed = 4;     // скорость уменьшена (было 5)

        // ---- Мир ----
        private Canvas worldCanvas;
        private List<Rect> obstacles = new List<Rect>();
        private List<GameObject> objects = new List<GameObject>();

        // ---- Затемнение ----
        private Rectangle darkOverlay;

        // ---- Ссылка на терминал ----
        private GameObject terminalObject;

        // ---- Флаг открытого терминала ----
        public bool IsTerminalOpen { get; private set; } = false;

        // ---- Переменные уровней ----
        private int currentLevel = 1;
        private bool exitDoorLocked = true;
        private int powerLevel = 0;
        private bool generatorActive = false;
        private int doorCode = 0;

        // ---- Выход по ESC ----
        private bool escPressed = false;
        private DateTime escPressStartTime;

        // ---- Фон и центрирование ----
        private Image roomBg;
        private TranslateTransform roomTransform;

        public MainWindow()
        {
            InitializeComponent();

            // Фон комнаты (не растягивается, исходный размер)
            roomBg = new Image();
            roomBg.Source = new BitmapImage(new Uri("pack://application:,,,/images/room.png"));
            roomBg.Stretch = Stretch.None;
            GameCanvas.Children.Add(roomBg);
            Canvas.SetZIndex(roomBg, -2);

            // Мировой канвас (объекты)
            worldCanvas = new Canvas { Background = Brushes.Transparent };
            GameCanvas.Children.Add(worldCanvas);

            // Трансформация для центрирования комнаты
            roomTransform = new TranslateTransform();
            worldCanvas.RenderTransform = roomTransform;

            // Затемнение
            darkOverlay = new Rectangle
            {
                Width = 800,
                Height = 600,
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
            };
            darkOverlay.Visibility = Visibility.Collapsed;
            worldCanvas.Children.Add(darkOverlay);
            Canvas.SetZIndex(darkOverlay, 100);

            LoadLevel(currentLevel);
            CompositionTarget.Rendering += GameLoop;
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

            double imgWidth = roomBg.Source.Width;
            double imgHeight = roomBg.Source.Height;
            double left = (canvasWidth - imgWidth) / 2;
            double top = (canvasHeight - imgHeight) / 2;
            Canvas.SetLeft(roomBg, left);
            Canvas.SetTop(roomBg, top);

            double roomLeft = (canvasWidth - 800) / 2;
            double roomTop = (canvasHeight - 600) / 2;
            roomTransform.X = roomLeft;
            roomTransform.Y = roomTop;
        }
    }
}
