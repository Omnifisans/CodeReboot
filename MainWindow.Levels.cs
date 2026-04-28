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
        // LoadLevel ВСЕГДА сбрасывает состояние головоломок
        private void LoadLevel(int level)
        {
            worldCanvas.Children.Clear();
            obstacles.Clear();
            objects.Clear();
            terminalObject = null;
            doorObject = null;

            // Сброс переменных уровня (происходит при каждой загрузке уровня)
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

            if (level == 1)
            {
                CreateWalls(); CreatePlayer(200, 350);
                CreateObject("terminal", 180, 150); CreateObject("generator", 550, 300);
                CreateDoor(false);
            }
            else if (level == 2)
            {
                CreateWalls(); CreatePlayer(200, 350);
                CreateObject("terminal", 500, 100); CreateObject("generator", 100, 400);
                CreateDoor(false);
            }
            else if (level == 3)
            {
                CreateWalls(); CreatePlayer(200, 350);
                CreateObject("terminal", 300, 200); CreateObject("generator", 500, 300);
                CreateDoor(false); CreateHint(150, 500);
            }
            else if (level == 4)
            {
                CreateWalls(); CreatePlayer(200, 350);
                CreateObject("terminal", 300, 200, true); CreateObject("generator", 500, 300);
                CreateDoor(false);
            }
            else if (level == 5)
            {
                CreateWalls(); CreatePlayer(200, 350);
                CreateObject("terminal", 300, 200); CreateObject("generator", 500, 300);
                CreateDoor(true); CreateCodePiece(700, 100, 1); CreateCodePiece(100, 500, 2);
            }

            UpdateTerminalImage();
        }

        private void CreateWalls()
        {
            obstacles.Add(new Rect(0, 0, 800, 1));
            obstacles.Add(new Rect(0, 599, 800, 1));
            obstacles.Add(new Rect(-140, 0, 1, 600));
            obstacles.Add(new Rect(930, 0, 1, 600));
        }

        private void CreatePlayer(double x, double y)
        {
            playerX = x; playerY = y;
            player = new Image { Width = 150, Height = 150 };
            player.Source = new BitmapImage(new Uri("pack://application:,,,/images/robot.png"));
            Canvas.SetLeft(player, playerX); Canvas.SetTop(player, playerY);
            worldCanvas.Children.Add(player); Canvas.SetZIndex(player, 200);
        }

        private void CreateObject(string type, double x, double y, bool alwaysOff = false)
        {
            Image img = new Image { Width = 180, Height = 180 };
            string uri = type == "terminal" ? (alwaysOff ? "terminal_off.png" : (generatorActive ? "terminal_on.png" : "terminal_off.png"))
                                            : (type == "generator" ? "generator.png" : "");
            if (string.IsNullOrEmpty(uri)) return;

            img.Source = new BitmapImage(new Uri($"pack://application:,,,/images/{uri}"));
            Canvas.SetLeft(img, x); Canvas.SetTop(img, y);
            worldCanvas.Children.Add(img);

            double hitboxW = type == "generator" ? 90 : 50;
            double hitboxH = type == "generator" ? 60 : 65;
            double offsetY = type == "generator" ? -25 : -15;
            obstacles.Add(new Rect(x + (180 - hitboxW) / 2, y + (180 - hitboxH) / 2 + offsetY, hitboxW, hitboxH));

            var obj = new GameObject { Visual = img, X = x, Y = y, Type = type };
            objects.Add(obj);
            if (type == "terminal") terminalObject = obj;
        }

        private void CreateDoor(bool hidden)
        {
            var doorImg = new Image { Width = 180, Height = 180 };
            doorImg.Source = new BitmapImage(new Uri("pack://application:,,,/images/door_closed.png"));
            double doorX = (800 - 180) / 2, doorY = -120;
            Canvas.SetLeft(doorImg, doorX); Canvas.SetTop(doorImg, doorY);
            if (hidden) doorImg.Visibility = Visibility.Collapsed;
            worldCanvas.Children.Add(doorImg);

            if (!hidden)
            {
                obstacles.Add(new Rect(doorX + 65, doorY + 65, 50, 50));
                obstacles.Add(new Rect(doorX + 65, 0, 50, 1));
            }
            doorObject = new GameObject { Visual = doorImg, X = doorX, Y = doorY, Type = "exit", Data = doorImg };
            objects.Add(doorObject);
        }

        private void CreateHint(double x, double y)
        {
            var hintImg = new Image { Width = 60, Height = 60 };
            hintImg.Source = new BitmapImage(new Uri("pack://application:,,,/images/note.png"));
            Canvas.SetLeft(hintImg, x); Canvas.SetTop(hintImg, y);
            worldCanvas.Children.Add(hintImg);
            objects.Add(new GameObject { Visual = hintImg, X = x, Y = y, Type = "hint" });
        }

        private void CreateCodePiece(double x, double y, int id)
        {
            var pieceImg = new Image { Width = 40, Height = 40 };
            pieceImg.Source = new BitmapImage(new Uri($"pack://application:,,,/images/code_piece{id}.png"));
            Canvas.SetLeft(pieceImg, x); Canvas.SetTop(pieceImg, y);
            worldCanvas.Children.Add(pieceImg);
            objects.Add(new GameObject { Visual = pieceImg, X = x, Y = y, Type = "codePiece", Data = id });
        }

        public void OpenDoor()
        {
            Image doorImg = doorObject?.Visual as Image;
            if (doorImg == null) return;

            doorImg.Source = new BitmapImage(new Uri("pack://application:,,,/images/door_open.png"));
            double dx = doorObject.X + 65, dy = doorObject.Y + 65;
            obstacles.RemoveAll(r => r.X == dx && r.Y == dy);
            obstacles.RemoveAll(r => r.X == dx && r.Y == 0);
        }

        public void ActivateDoorOnLevel5()
        {
            if (currentLevel == 5 && doorObject?.Visual is Image doorImg)
            {
                doorImg.Visibility = Visibility.Visible;
                obstacles.Add(new Rect(doorObject.X + 65, doorObject.Y + 65, 50, 50));
                obstacles.Add(new Rect(doorObject.X + 65, 0, 50, 1));
                doorSpawned = true;
                if (!exitDoorLocked) OpenDoor();
            }
        }

        private void UpdateTerminalImage()
        {
            if (terminalObject?.Visual is Image img)
            {
                if (currentLevel == 4) { img.Source = new BitmapImage(new Uri("pack://application:,,,/images/terminal_off.png")); return; }
                img.Source = new BitmapImage(new Uri($"pack://application:,,,/images/{(generatorActive ? "terminal_on.png" : "terminal_off.png")}"));
            }
        }
    }
}
