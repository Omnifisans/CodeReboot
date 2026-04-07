using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CodeRebootWPF
{
    public partial class MainWindow : Window
    {
        // ---- Игрок ----
        private Rectangle player;
        private double playerX = 200;
        private double playerY = 200;
        private double playerSpeed = 5;

        private bool moveUp, moveDown, moveLeft, moveRight;

        // ---- Мир ----
        private Canvas worldCanvas;
        private TranslateTransform worldTransform;
        private List<Rect> obstacles = new List<Rect>();
        private List<GameObject> objects = new List<GameObject>();

        // ---- Переменные уровня ----
        private int currentLevel = 1;
        private bool exitDoorLocked = true;
        private int powerLevel = 0;
        private bool generatorActive = false;
        private int doorCode = 0;

        // ---- Выход по ESC ----
        private bool escPressed = false;
        private DateTime escPressStartTime;

        public MainWindow()
        {
            InitializeComponent();
            worldCanvas = new Canvas { Background = Brushes.Transparent };
            GameCanvas.Children.Add(worldCanvas);
            worldTransform = new TranslateTransform();
            worldCanvas.RenderTransform = worldTransform;

            LoadLevel(currentLevel);
            CompositionTarget.Rendering += GameLoop;
            GameCanvas.Focus();
        }

        private void LoadLevel(int level)
        {
            worldCanvas.Children.Clear();
            obstacles.Clear();
            objects.Clear();

            if (level == 1)
            {
                exitDoorLocked = true;
                powerLevel = 0;
                generatorActive = false;
                doorCode = 0;

                CreateWalls();
                CreatePlayer(200, 200);
                CreateObject(Brushes.Cyan, 300, 150, "terminal");
                CreateObject(Brushes.Yellow, 500, 300, "generator");
                CreateObject(Brushes.Green, 740, 200, "exit");
            }
            else if (level == 2)
            {
                exitDoorLocked = true;
                powerLevel = 0;
                generatorActive = false;
                doorCode = 0;

                CreateWalls();
                CreatePlayer(200, 200);
                CreateObject(Brushes.Cyan, 400, 100, "terminal");
                CreateObject(Brushes.Yellow, 100, 400, "generator");
                CreateObject(Brushes.Green, 700, 500, "exit");
            }

            UpdateUI();
        }

        private void CreateWalls()
        {
            AddWall(0, 0, 800, 10);
            AddWall(0, 590, 800, 10);
            AddWall(0, 0, 10, 600);
            AddWall(790, 0, 10, 600);
        }

        private void AddWall(double x, double y, double width, double height)
        {
            var wall = new Rectangle { Width = width, Height = height, Fill = Brushes.White };
            Canvas.SetLeft(wall, x);
            Canvas.SetTop(wall, y);
            worldCanvas.Children.Add(wall);
            obstacles.Add(new Rect(x, y, width, height));
        }

        private void CreatePlayer(double x, double y)
        {
            playerX = x;
            playerY = y;
            player = new Rectangle
            {
                Width = 40,
                Height = 40,
                Fill = Brushes.Lime,
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
            Canvas.SetLeft(player, playerX);
            Canvas.SetTop(player, playerY);
            worldCanvas.Children.Add(player);
        }

        private void CreateObject(SolidColorBrush color, double x, double y, string type)
        {
            var rect = new Rectangle
            {
                Width = 50,
                Height = 50,
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            worldCanvas.Children.Add(rect);
            obstacles.Add(new Rect(x, y, 50, 50));
            objects.Add(new GameObject { Visual = rect, X = x, Y = y, Type = type });
        }

        internal bool GetExitDoorLocked() => exitDoorLocked;
        internal void SetExitDoorLocked(bool value) => exitDoorLocked = value;
        internal int GetPowerLevel() => powerLevel;
        internal void SetPowerLevel(int value) => powerLevel = value;
        internal bool GetGeneratorActive() => generatorActive;
        internal void SetGeneratorActive(bool value) => generatorActive = value;
        internal int GetDoorCode() => doorCode;
        internal void SetDoorCode(int value) => doorCode = value;
        internal int GetCurrentLevel() => currentLevel;

        private void GameLoop(object sender, EventArgs e)
        {
            double newX = playerX;
            double newY = playerY;

            if (moveUp) newY -= playerSpeed;
            if (moveDown) newY += playerSpeed;
            if (moveLeft) newX -= playerSpeed;
            if (moveRight) newX += playerSpeed;

            Rect newRect = new Rect(newX, newY, 40, 40);
            bool collision = false;
            foreach (var obs in obstacles)
            {
                if (newRect.IntersectsWith(obs))
                {
                    collision = true;
                    break;
                }
            }

            if (!collision)
            {
                playerX = newX;
                playerY = newY;
            }

            Canvas.SetLeft(player, playerX);
            Canvas.SetTop(player, playerY);
            CenterCamera();
            CheckInteraction();
            UpdateUI();

            if (escPressed && (DateTime.Now - escPressStartTime).TotalSeconds >= 2.0)
                Application.Current.Shutdown();
        }

        private void CenterCamera()
        {
            if (worldCanvas == null) return;
            double w = GameCanvas.ActualWidth;
            double h = GameCanvas.ActualHeight;
            double targetX = w / 2 - (playerX + 20);
            double targetY = h / 2 - (playerY + 20);
            worldTransform.X = targetX;
            worldTransform.Y = targetY;
        }

        private void GameCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W: case Key.Up: moveUp = true; break;
                case Key.S: case Key.Down: moveDown = true; break;
                case Key.A: case Key.Left: moveLeft = true; break;
                case Key.D: case Key.Right: moveRight = true; break;
                case Key.E: case Key.Enter: Interact(); break;
                case Key.F4: ToggleFullScreen(); break;
                case Key.Escape:
                    if (!escPressed) { escPressed = true; escPressStartTime = DateTime.Now; }
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W: case Key.Up: moveUp = false; break;
                case Key.S: case Key.Down: moveDown = false; break;
                case Key.A: case Key.Left: moveLeft = false; break;
                case Key.D: case Key.Right: moveRight = false; break;
                case Key.Escape: escPressed = false; break;
            }
            base.OnKeyUp(e);
        }

        private void ToggleFullScreen()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            CenterCamera();
        }

        private void CheckInteraction()
        {
            foreach (var obj in objects)
            {
                double dx = playerX - obj.X;
                double dy = playerY - obj.Y;
                obj.Nearby = Math.Sqrt(dx * dx + dy * dy) < 60;
            }
        }

        private void Interact()
        {
            foreach (var obj in objects)
            {
                if (obj.Nearby)
                {
                    switch (obj.Type)
                    {
                        case "terminal":
                            if (!generatorActive)
                                MessageBox.Show("Терминал обесточен. Включите генератор.", "Нет энергии");
                            else
                                OpenCodeEditor();
                            break;

                        case "generator":
                            generatorActive = !generatorActive;
                            powerLevel = generatorActive ? 5 : 0;
                            MessageBox.Show(generatorActive ? "Генератор включён. Энергия: 5" : "Генератор выключен. Энергия: 0", "Генератор");
                            break;

                        case "exit":
                            if (currentLevel == 1)
                            {
                                if (!exitDoorLocked)
                                {
                                    MessageBox.Show("Уровень 1 пройден! Переход на уровень 2...", "Победа");
                                    currentLevel = 2;
                                    LoadLevel(2);
                                }
                                else
                                    MessageBox.Show("Выход заблокирован. Используйте терминал, чтобы изменить exitDoorLocked.", "Выход");
                            }
                            else if (currentLevel == 2)
                            {
                                if (!exitDoorLocked && doorCode == 42)
                                {
                                    MessageBox.Show("Поздравляем! Игра пройдена!", "Финал");
                                    Application.Current.Shutdown();
                                }
                                else
                                {
                                    string msg = "Выход закрыт.\n";
                                    if (exitDoorLocked) msg += "- Переменная exitDoorLocked = true\n";
                                    if (doorCode != 42) msg += "- Неверный код доступа. Подсказка: ответ на главный вопрос жизни, вселенной и всего такого.";
                                    MessageBox.Show(msg, "Выход");
                                }
                            }
                            break;
                    }
                    break;
                }
            }
        }

        private void OpenCodeEditor()
        {
            var dialog = new TerminalEditorWindow(this);
            dialog.ShowDialog();
        }

        private void UpdateUI()
        {
            DoorStatusText.Text = $"Exit_Door.Locked = {exitDoorLocked}";
            PowerStatusText.Text = $"Энергия: {powerLevel}";
            GeneratorStatusText.Text = $"Генератор: {(generatorActive ? "вкл" : "выкл")}";
            CodeStatusText.Text = currentLevel == 2 ? $"Door Code: {doorCode}" : "";
        }
    }

    public class GameObject
    {
        public Rectangle Visual { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Type { get; set; }
        public bool Nearby { get; set; }
    }

    internal class TerminalEditorWindow : Window
    {
        private MainWindow mainWindow;
        private List<VariableInfo> variables;
        private int selectedIndex = 0;
        private StackPanel panel;
        private bool isEditing = false;
        private string currentVarName;
        private string currentVarType;
        private object currentValue;

        public TerminalEditorWindow(MainWindow owner)
        {
            mainWindow = owner;
            Owner = owner;
            Title = "Терминал";
            Width = 450;
            Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            if (mainWindow.GetCurrentLevel() == 1)
            {
                variables = new List<VariableInfo>
                {
                    new VariableInfo { Name = "exitDoorLocked", Type = "bool" }
                };
            }
            else
            {
                variables = new List<VariableInfo>
                {
                    new VariableInfo { Name = "exitDoorLocked", Type = "bool" },
                    new VariableInfo { Name = "doorCode", Type = "int" }
                };
            }

            panel = new StackPanel { Margin = new Thickness(10) };
            Content = panel;
            ShowVariableList();
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void ShowVariableList()
        {
            panel.Children.Clear();
            isEditing = false;
            panel.Children.Add(new TextBlock
            {
                Text = "Выберите переменную (↑↓ - перемещение, Enter - редактировать, Esc - выход):",
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            });

            for (int i = 0; i < variables.Count; i++)
            {
                var varInfo = variables[i];
                object currentValue = null;
                if (varInfo.Name == "exitDoorLocked") currentValue = mainWindow.GetExitDoorLocked();
                else if (varInfo.Name == "doorCode") currentValue = mainWindow.GetDoorCode();

                var tb = new TextBlock
                {
                    Text = $"{varInfo.Name} ({varInfo.Type}) = {currentValue}",
                    Margin = new Thickness(0, 2, 0, 2),
                    Padding = new Thickness(2)
                };
                if (i == selectedIndex)
                    tb.Background = Brushes.LightBlue;
                panel.Children.Add(tb);
            }
        }

        private void ShowEditPanel(string varName, string varType, object currentValue)
        {
            panel.Children.Clear();
            isEditing = true;
            currentVarName = varName;
            currentVarType = varType;
            this.currentValue = currentValue;

            panel.Children.Add(new TextBlock
            {
                Text = $"Редактирование: {varName} ({varType})",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });
            panel.Children.Add(new TextBlock { Text = "Введите новое значение:", Margin = new Thickness(0, 0, 0, 5) });

            var txtBox = new TextBox
            {
                Text = currentValue.ToString(),
                Margin = new Thickness(0, 0, 0, 10),
                MinWidth = 200,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            txtBox.SelectAll();
            panel.Children.Add(txtBox);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okBtn = new Button { Content = "OK", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            var cancelBtn = new Button { Content = "Отмена", Width = 80, Height = 30, IsCancel = true };
            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            panel.Children.Add(btnPanel);

            okBtn.Click += (s, e) => TrySaveAndClose(txtBox.Text);
            cancelBtn.Click += (s, e) => ShowVariableList();

            // ИСПРАВЛЕННЫЙ ОБРАБОТЧИК: Handled только для Enter/Escape
            txtBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    TrySaveAndClose(txtBox.Text);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    ShowVariableList();
                    e.Handled = true;
                }
                // для остальных клавиш не трогаем Handled
            };

            txtBox.Focus();
        }

        private void TrySaveAndClose(string input)
        {
            try
            {
                if (currentVarType == "bool")
                {
                    if (bool.TryParse(input.Trim().ToLower(), out bool newValue))
                    {
                        if (currentVarName == "exitDoorLocked")
                            mainWindow.SetExitDoorLocked(newValue);
                        DialogResult = true;
                        Close();
                    }
                    else
                        MessageBox.Show("Ошибка: введите true или false", "Неверное значение");
                }
                else if (currentVarType == "int")
                {
                    if (int.TryParse(input.Trim(), out int newValue))
                    {
                        mainWindow.SetDoorCode(newValue);
                        DialogResult = true;
                        Close();
                    }
                    else
                        MessageBox.Show("Ошибка: введите целое число", "Неверное значение");
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!isEditing)
            {
                if (e.Key == Key.Up) { selectedIndex = (selectedIndex - 1 + variables.Count) % variables.Count; ShowVariableList(); e.Handled = true; }
                else if (e.Key == Key.Down) { selectedIndex = (selectedIndex + 1) % variables.Count; ShowVariableList(); e.Handled = true; }
                else if (e.Key == Key.Enter)
                {
                    var selected = variables[selectedIndex];
                    object val = null;
                    if (selected.Name == "exitDoorLocked") val = mainWindow.GetExitDoorLocked();
                    else if (selected.Name == "doorCode") val = mainWindow.GetDoorCode();
                    ShowEditPanel(selected.Name, selected.Type, val);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape) { DialogResult = false; Close(); e.Handled = true; }
            }
        }

        private class VariableInfo { public string Name { get; set; } public string Type { get; set; } }
    }
}