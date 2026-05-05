using System;
using System.Windows;
using System.Windows.Input;

namespace CodeRebootWPF
{
    public partial class MainWindow
    {
        private bool moveUp, moveDown, moveLeft, moveRight;

        private void GameCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsTerminalOpen) return;
            switch (e.Key)
            {
                case Key.W: case Key.Up: moveUp = true; break;
                case Key.S: case Key.Down: moveDown = true; break;
                case Key.A: case Key.Left: moveLeft = true; break;
                case Key.D: case Key.Right: moveRight = true; break;
                case Key.E: case Key.Enter: Interact(); break;
                case Key.F4: ToggleFullScreen(); break;
                case Key.Escape: if (!escPressed) { escPressed = true; escPressStartTime = DateTime.Now; } break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (IsTerminalOpen) return;
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
            if (this.WindowStyle == WindowStyle.None)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.ResizeMode = ResizeMode.CanResize;
                this.Width = 1024;
                this.Height = 768;
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.Left = 0;
                this.Top = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;
            }
            UpdateLayout();
            CenterRoom();
        }
    }
}
