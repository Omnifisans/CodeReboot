using System;
using System.Collections.Generic;
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

        public MenuWindow()
        {
            InitializeComponent();

            // Запуск в полный экран
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.NoResize;
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            menuButtons = new List<Button> { btnStart, btnContinue, btnControls, btnTests, btnExit };

            escTimer = new DispatcherTimer();
            escTimer.Interval = TimeSpan.FromMilliseconds(100);
            escTimer.Tick += EscTimer_Tick;
            escTimer.Start();

            UpdateSelection();
        }

        private void EscTimer_Tick(object sender, EventArgs e)
        {
            if (escPressed && (DateTime.Now - escPressStartTime).TotalSeconds >= 2.0)
                Application.Current.Shutdown();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down) { selectedIndex = (selectedIndex + 1) % menuButtons.Count; UpdateSelection(); e.Handled = true; }
            else if (e.Key == Key.Up) { selectedIndex = (selectedIndex - 1 + menuButtons.Count) % menuButtons.Count; UpdateSelection(); e.Handled = true; }
            else if (e.Key == Key.Enter) { if (menuButtons[selectedIndex].IsEnabled) menuButtons[selectedIndex].RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); e.Handled = true; }
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

        private void BtnStart_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); Close(); }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new MainWindow();
            gameWindow.LoadGame(); // Загружаем сохранение
            gameWindow.Show();
            Close();
        }

        private void BtnControls_Click(object sender, RoutedEventArgs e) => MessageBox.Show("WASD/Стрелки - движение, E/Enter - действие, F4 - полный экран, Esc (2 сек) - выход", "Управление");
        private void BtnExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void BtnTests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string report = GameTests.RunAll();
                MessageBox.Show(report, "🧪 Отчёт автотестов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при запуске тестов:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
