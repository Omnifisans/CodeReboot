using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeRebootWPF
{
    public partial class MainWindow
    {
        private void GameLoop(object sender, EventArgs e)
        {
            if (player == null) return;

            double newX = playerX;
            double newY = playerY;
            if (moveUp) newY -= playerSpeed;
            if (moveDown) newY += playerSpeed;
            if (moveLeft) newX -= playerSpeed;
            if (moveRight) newX += playerSpeed;

            double hitboxX = newX + (150 - 40) / 2;
            double hitboxY = newY + (150 - 40) / 2;
            Rect newRect = new Rect(hitboxX, hitboxY, 40, 40);

            bool collision = false;
            foreach (var obs in obstacles)
                if (newRect.IntersectsWith(obs)) { collision = true; break; }

            if (!collision)
            {
                playerX = newX;
                playerY = newY;
            }

            Canvas.SetLeft(player, playerX);
            Canvas.SetTop(player, playerY);

            // 🔧 ЛОГИКА АНИМАЦИИ
            bool isMoving = moveUp || moveDown || moveLeft || moveRight;

            if (isMoving)
            {
                // Определяем направление
                if (moveUp) currentDirection = "up";
                else if (moveDown) currentDirection = "down";
                else if (moveLeft) currentDirection = "left";
                else if (moveRight) currentDirection = "right";

                animCounter++;
                // Меняем кадр только когда счетчик превысил порог AnimSpeed
                int frame = (animCounter / AnimSpeed) % 2;
                player.Source = playerSprites[$"{currentDirection}_walk{frame + 1}"];
            }
            else
            {
                animCounter = 0;
                // Возвращаем спрайт покоя
                player.Source = currentDirection == "down"
                    ? playerSprites["down_stay"]
                    : playerSprites[$"{currentDirection}_stay"];
            }

            CheckInteraction();
            UpdateUI();

            darkOverlay.Visibility = generatorActive ? Visibility.Collapsed : Visibility.Visible;

            if (escPressed && (DateTime.Now - escPressStartTime).TotalSeconds >= 2.0)
                Application.Current.Shutdown();
        }

        private void UpdateUI()
        {
            DoorStatusText.Text = $"Exit_Door.Locked = {exitDoorLocked}";
            PowerStatusText.Text = $"Энергия: {powerLevel}";
            GeneratorStatusText.Text = $"Генератор: {(generatorActive ? "вкл" : "выкл")}";
            if (currentLevel == 2)
                CodeStatusText.Text = $"Door Code: {doorCode}";
            else if (currentLevel == 3)
                CodeStatusText.Text = $"Activation Code: {activationCode}";
            else if (currentLevel == 4)
                CodeStatusText.Text = $"Переключений: {generatorToggles}/5";
            else if (currentLevel == 5)
                CodeStatusText.Text = $"Части кода: {(hasPiece1 ? "✓" : "□")} {(hasPiece2 ? "✓" : "□")}";
            else
                CodeStatusText.Text = "";
        }
    }
}
