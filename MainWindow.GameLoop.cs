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
            double newX = playerX;
            double newY = playerY;
            if (moveUp) newY -= playerSpeed;
            if (moveDown) newY += playerSpeed;
            if (moveLeft) newX -= playerSpeed;
            if (moveRight) newX += playerSpeed;

            // Хитбокс 40x40 по центру спрайта 150x150
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
            else
                CodeStatusText.Text = "";
        }
    }
}