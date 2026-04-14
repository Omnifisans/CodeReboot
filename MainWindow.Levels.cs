using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CodeRebootWPF
{
    public partial class MainWindow
    {
        private void LoadLevel(int level)
        {
            worldCanvas.Children.Clear();
            obstacles.Clear();
            objects.Clear();
            terminalObject = null;

            if (level == 1)
            {
                exitDoorLocked = true;
                powerLevel = 0;
                generatorActive = false;
                doorCode = 0;

                CreateWalls();
                CreatePlayer(200, 350);
                CreateObject("terminal", 180, 150);
                CreateObject("generator", 550, 300);
                CreateDoor();
            }
            else if (level == 2)
            {
                exitDoorLocked = true;
                powerLevel = 0;
                generatorActive = false;
                doorCode = 0;

                CreateWalls();
                CreatePlayer(200, 350);
                CreateObject("terminal", 500, 100);
                CreateObject("generator", 100, 400);
                CreateDoor();
            }

            UpdateTerminalImage();
        }

        private void CreateWalls()
        {
            // Верхняя стена — сплошная, без разрыва
            obstacles.Add(new Rect(0, 0, 800, 1));
            // Нижняя стена
            obstacles.Add(new Rect(0, 599, 800, 1));
            // Левая стена
            obstacles.Add(new Rect(-140, 0, 1, 600));
            // Правая стена
            obstacles.Add(new Rect(930, 0, 1, 600));
        }

        private void CreatePlayer(double x, double y)
        {
            playerX = x;
            playerY = y;
            player = new Image();
            player.Width = 150;
            player.Height = 150;
            player.Source = new BitmapImage(new Uri("pack://application:,,,/images/robot.png"));
            Canvas.SetLeft(player, playerX);
            Canvas.SetTop(player, playerY);
            worldCanvas.Children.Add(player);
            Canvas.SetZIndex(player, 200);
        }

        private void CreateObject(string type, double x, double y)
        {
            Image img = new Image();
            img.Width = 180;
            img.Height = 180;
            string uri = "";
            switch (type)
            {
                case "terminal":
                    uri = generatorActive ? "terminal_on.png" : "terminal_off.png";
                    break;
                case "generator":
                    uri = "generator.png";
                    break;
                default:
                    return;
            }
            img.Source = new BitmapImage(new Uri($"pack://application:,,,/images/{uri}"));
            Canvas.SetLeft(img, x);
            Canvas.SetTop(img, y);
            worldCanvas.Children.Add(img);

            double hitboxW, hitboxH;
            double offsetY = 0;
            if (type == "generator")
            {
                hitboxW = 90;
                hitboxH = 60;
                offsetY = -25;
            }
            else
            {
                hitboxW = 50;
                hitboxH = 65;
                offsetY = -15;
            }
            double hitboxX = x + (180 - hitboxW) / 2;
            double hitboxY = y + (180 - hitboxH) / 2 + offsetY;
            obstacles.Add(new Rect(hitboxX, hitboxY, hitboxW, hitboxH));

            var obj = new GameObject { Visual = img, X = x, Y = y, Type = type };
            objects.Add(obj);
            if (type == "terminal")
                terminalObject = obj;
        }

        private void CreateDoor()
        {
            var doorImg = new Image();
            doorImg.Width = 180;
            doorImg.Height = 180;
            doorImg.Source = new BitmapImage(new Uri("pack://application:,,,/images/door_closed.png"));
            double doorX = (800 - 180) / 2; // 310
            double doorY = -120;            // высокая дверь
            Canvas.SetLeft(doorImg, doorX);
            Canvas.SetTop(doorImg, doorY);
            worldCanvas.Children.Add(doorImg);

            // Хитбокс для взаимодействия с дверью (на уровне спрайта)
            double hitboxX = doorX + (180 - 50) / 2;
            double hitboxY = doorY + (180 - 50) / 2;
            obstacles.Add(new Rect(hitboxX, hitboxY, 50, 50));

            // Дополнительный хитбокс в проёме (на уровне пола, чтобы закрыть дыру)
            double blockX = doorX + (180 - 50) / 2;
            double blockY = 0;  // на уровне верхней стены
            obstacles.Add(new Rect(blockX, blockY, 50, 1)); // тонкий, но блокирует проход

            objects.Add(new GameObject { Visual = doorImg, X = doorX, Y = doorY, Type = "exit", Data = doorImg });
        }

        public void OpenDoor()
        {
            foreach (var obj in objects)
            {
                if (obj.Type == "exit")
                {
                    var doorImg = obj.Visual as Image;
                    if (doorImg != null)
                    {
                        doorImg.Source = new BitmapImage(new Uri("pack://application:,,,/images/door_open.png"));
                    }
                    // Удаляем оба хитбокса: основной и дополнительный
                    double hitboxX = obj.X + (180 - 50) / 2;
                    double hitboxY = obj.Y + (180 - 50) / 2;
                    obstacles.RemoveAll(r => r.X == hitboxX && r.Y == hitboxY);
                    double blockX = obj.X + (180 - 50) / 2;
                    double blockY = 0;
                    obstacles.RemoveAll(r => r.X == blockX && r.Y == blockY);
                    break;
                }
            }
        }

        private void UpdateTerminalImage()
        {
            if (terminalObject != null && terminalObject.Visual is Image img)
            {
                string uri = generatorActive ? "terminal_on.png" : "terminal_off.png";
                img.Source = new BitmapImage(new Uri($"pack://application:,,,/images/{uri}"));
            }
        }
    }
}