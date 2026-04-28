using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CodeRebootWPF
{
    public partial class MenuWindow : Window
    {
        private List<Button> menuButtons;
        private int selectedIndex = 0;
        private bool escPressed = false;
        private DateTime escPressStartTime;
        private DispatcherTimer escTimer;

        // Путь к файлу сохранения (должен совпадать с MainWindow)
        private static readonly string SavePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeRebootWPF", "save.xml");

        public MenuWindow()
        {
            InitializeComponent();

            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.NoResize;
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            menuButtons = new List<Button> { btnStart, btnContinue, btnControls, btnExit };

            // 🔧 ПРОВЕРКА: Если есть сохранение, включаем кнопку
            UpdateContinueButton();

            escTimer = new DispatcherTimer();
            escTimer.Interval = TimeSpan.FromMilliseconds(100);
            escTimer.Tick += EscTimer_Tick;
            escTimer.Start();

            UpdateSelection();
        }

        private void UpdateContinueButton()
        {
            bool hasSave = File.Exists(SavePath);
            btnContinue.IsEnabled = hasSave;

            // Визуальное обновление (белый если активна, серый если нет)
            if (hasSave)
                btnContinue.Foreground = Brushes.White;
            else
                btnContinue.Foreground = Brushes.Gray;
        }

        private void EscTimer_Tick(object sender, EventArgs e)
        {
            if (escPressed && (DateTime.Now - escPressStartTime).TotalSeconds >= 2.0)
                Application.Current.Shutdown();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Навигация с пропуском отключенных кнопок
            if (e.Key == Key.Down)
            {
                do { selectedIndex = (selectedIndex + 1) % menuButtons.Count; }
                while (!menuButtons[selectedIndex].IsEnabled && menuButtons.Count > 1);
                UpdateSelection(); e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                do { selectedIndex = (selectedIndex - 1 + menuButtons.Count) % menuButtons.Count; }
                while (!menuButtons[selectedIndex].IsEnabled && menuButtons.Count > 1);
                UpdateSelection(); e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (menuButtons[selectedIndex].IsEnabled)
                    menuButtons[selectedIndex].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
            else if (e.Key == Key.F4) { ToggleFullScreen(); e.Handled = true; }
            else if (e.Key == Key.Escape) { if (!escPressed) { escPressed = true; escPressStartTime = DateTime.Now; } e.Handled = true; }
        }

        protected override void OnKeyUp(KeyEventArgs e) { if (e.Key == Key.Escape) escPressed = false; base.OnKeyUp(e); }

        private void ToggleFullScreen()
        {
            if (this.WindowStyle == WindowStyle.None)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.ResizeMode = ResizeMode.CanResize;
                this.Width = 1024; this.Height = 768;
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.Left = 0; this.Top = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;
            }
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < menuButtons.Count; i++)
            {
                var btn = menuButtons[i];
                var st = (ScaleTransform)btn.RenderTransform;
                if (!btn.IsEnabled) { st.ScaleX = 1; st.ScaleY = 1; btn.Foreground = Brushes.Gray; }
                else if (i == selectedIndex) { st.ScaleX = 1.2; st.ScaleY = 1.2; btn.Foreground = Brushes.LimeGreen; }
                else { st.ScaleX = 1; st.ScaleY = 1; btn.Foreground = Brushes.White; }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // Удаляем старое сохранение при новой игре
            if (File.Exists(SavePath)) File.Delete(SavePath);
            new MainWindow().Show();
            Close();
        }

        // 🔧 НОВЫЙ ОБРАБОТЧИК: Кнопка "Продолжить"
        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(SavePath))
            {
                new MainWindow().Show();
                Close();
            }
        }

        private void BtnControls_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show("WASD/Стрелки - движение\nE/Enter - действие\nF4 - полный экран\nEsc (2 сек) - выход\n\n💾 Игра сохраняется автоматически после прохождения каждого уровня.", "Управление");

        private void BtnExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}