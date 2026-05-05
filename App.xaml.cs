using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CodeRebootWPF
{
    public partial class App : Application
    {
        public static readonly MediaPlayer BgmPlayer = new MediaPlayer();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 1. Получаем встроенный ресурс из EXE
                var resourceUri = new Uri("pack://application:,,,/music/background_levels_music.MP3");
                var streamInfo = Application.GetResourceStream(resourceUri);

                if (streamInfo == null)
                {
                    MessageBox.Show("Музыка не найдена в ресурсах!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    new MenuWindow().Show();
                    return;
                }

                // 2. Копируем во временную папку (MediaPlayer не играет из pack://)
                string tempMusicPath = Path.Combine(Path.GetTempPath(), "CodeReboot_Music.mp3");
                using (var reader = streamInfo.Stream)
                using (var writer = new FileStream(tempMusicPath, FileMode.Create))
                {
                    reader.CopyTo(writer);
                }

                // 3. Запускаем воспроизведение
                BgmPlayer.Open(new Uri(tempMusicPath));
                BgmPlayer.Volume = 0.5;

                // Зацикливание
                BgmPlayer.MediaEnded += (s, args) =>
                {
                    BgmPlayer.Position = TimeSpan.Zero;
                    BgmPlayer.Play();
                };

                BgmPlayer.Play();
            }
            catch (Exception ex)
            {
                // Если музыка не загрузилась — игра всё равно запустится
                System.Diagnostics.Debug.WriteLine("Music error: " + ex.Message);
            }

            new MenuWindow().Show();
        }
    }
}
