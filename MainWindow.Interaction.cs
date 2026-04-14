using System;
using System.Windows;

namespace CodeRebootWPF
{
    public partial class MainWindow
    {
        private void CheckInteraction()
        {
            foreach (var obj in objects)
            {
                double dx = playerX - obj.X;
                double dy = playerY - obj.Y;
                obj.Nearby = Math.Sqrt(dx * dx + dy * dy) < 80; // дистанция для взаимодействия
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
                            UpdateTerminalImage();
                            break;
                        case "exit":
                            CheckExit();
                            break;
                    }
                    break;
                }
            }
        }

        private void CheckExit()
        {
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
        }

        private void OpenCodeEditor()
        {
            IsTerminalOpen = true;
            var dialog = new TerminalEditorWindow(this);
            dialog.ShowDialog();
            IsTerminalOpen = false;
        }
    }
}