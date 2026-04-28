using System;
using System.Windows;
using System.Windows.Media;

namespace CodeRebootWPF
{
    public partial class App : Application
    {
        // Глобальный плеер, доступный из любой части игры
        public static MediaPlayer GlobalMusicPlayer { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Инициализация музыки
            GlobalMusicPlayer = new MediaPlayer();

            // Путь к файлу (настроен в .csproj для копирования в папку music)
            GlobalMusicPlayer.Open(new Uri("music/background_levels_music.MP3", UriKind.Relative));

            // 2. Зацикливание: когда трек заканчивается, сбрасываем на 0 и играем снова
            GlobalMusicPlayer.MediaEnded += (s, args) =>
            {
                GlobalMusicPlayer.Position = TimeSpan.Zero;
                GlobalMusicPlayer.Play();
            };

            // 3. Настройка и запуск
            GlobalMusicPlayer.Volume = 0.3; // Громкость 30%
            GlobalMusicPlayer.Play();

            // 4. Запуск главного меню вручную
            new MenuWindow().Show();
        }
    }
}
