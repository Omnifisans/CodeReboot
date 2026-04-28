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
                obj.Nearby = Math.Sqrt(dx * dx + dy * dy) < 100;
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
                            if (currentLevel == 4)
                            {
                                MessageBox.Show("Терминал не включается. Комната старая, энергии нет.", "Терминал");
                                return;
                            }
                            if (!generatorActive)
                                MessageBox.Show("Терминал обесточен. Включите генератор.", "Нет энергии");
                            else
                                OpenCodeEditor();
                            break;

                        case "generator":
                            if (currentLevel == 4)
                            {
                                if (generatorBroken)
                                {
                                    MessageBox.Show("Генератор сломался. Дверь должна открыться...", "Генератор");
                                }
                                else
                                {
                                    generatorActive = !generatorActive;
                                    if (generatorActive)
                                    {
                                        powerLevel = 5;
                                        generatorToggles++;
                                        MessageBox.Show($"Генератор включён. Осталось энергии: {5 - generatorToggles}", "Генератор");
                                    }
                                    else
                                    {
                                        powerLevel = 0;
                                        generatorToggles++;
                                        MessageBox.Show($"Генератор выключен. Осталось энергии: {5 - generatorToggles}", "Генератор");
                                    }
                                    if (generatorToggles >= 5)
                                    {
                                        generatorBroken = true;
                                        SetExitDoorLocked(false);
                                        MessageBox.Show("Генератор сломался! Дверь открыта.", "Успех");
                                    }
                                }
                            }
                            else
                            {
                                generatorActive = !generatorActive;
                                powerLevel = generatorActive ? 5 : 0;
                                MessageBox.Show(generatorActive ? "Генератор включён. Энергия: 5" : "Генератор выключен. Энергия: 0", "Генератор");
                                UpdateTerminalImage();
                            }
                            break;

                        case "exit":
                            CheckExit();
                            break;

                        case "hint":
                            if (currentLevel == 3)
                            {
                                MessageBox.Show("Записка: «Код активации: DEBUG»", "Подсказка");
                                objects.Remove(obj);
                                worldCanvas.Children.Remove(obj.Visual as UIElement);
                            }
                            break;

                        case "codePiece":
                            int pieceId = (int)obj.Data;
                            if (pieceId == 1 && !hasPiece1)
                            {
                                hasPiece1 = true;
                                objects.Remove(obj);
                                worldCanvas.Children.Remove(obj.Visual as UIElement);
                                MessageBox.Show("Вы нашли часть кода: «exitDoorLocked»", "Код");
                            }
                            else if (pieceId == 2 && !hasPiece2)
                            {
                                hasPiece2 = true;
                                objects.Remove(obj);
                                worldCanvas.Children.Remove(obj.Visual as UIElement);
                                MessageBox.Show("Вы нашли часть кода: «= True»", "Код");
                            }
                            if (hasPiece1 && hasPiece2 && !doorSpawned && currentLevel == 5)
                            {
                                ActivateDoorOnLevel5();
                                MessageBox.Show("Дверь выхода материализовалась!", "Появился выход");
                            }
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
                    SaveGame(); // 💾 Сохраняем перед переходом
                    currentLevel = 2;
                    LoadLevel(2);
                }
                else MessageBox.Show("Выход заблокирован. Используйте терминал.", "Выход");
            }
            else if (currentLevel == 2)
            {
                if (!exitDoorLocked && doorCode == 42)
                {
                    MessageBox.Show("Уровень 2 пройден! Переход на уровень 3...", "Победа");
                    SaveGame(); // 💾
                    currentLevel = 3;
                    LoadLevel(3);
                }
                else
                {
                    string msg = "Выход закрыт.\n";
                    if (exitDoorLocked) msg += "- exitDoorLocked = true\n";
                    if (doorCode != 42) msg += "- Неверный код (подсказка: 42)";
                    MessageBox.Show(msg, "Выход");
                }
            }
            else if (currentLevel == 3)
            {
                if (!exitDoorLocked)
                {
                    MessageBox.Show("Уровень 3 пройден! Переход на уровень 4...", "Победа");
                    SaveGame(); // 💾
                    currentLevel = 4;
                    LoadLevel(4);
                }
                else MessageBox.Show("Выход требует код активации.", "Выход");
            }
            else if (currentLevel == 4)
            {
                if (!exitDoorLocked)
                {
                    MessageBox.Show("Уровень 4 пройден! Переход на уровень 5...", "Победа");
                    SaveGame(); // 💾
                    currentLevel = 5;
                    LoadLevel(5);
                }
                else MessageBox.Show("Выход ещё не открыт.", "Выход");
            }
            else if (currentLevel == 5)
            {
                if (!exitDoorLocked && doorSpawned)
                {
                    MessageBox.Show("Поздравляем! Игра пройдена!", "Финал");
                    Application.Current.Shutdown();
                }
                else MessageBox.Show("Выход закрыт.", "Выход");
            }
        }

        private void OpenCodeEditor()
        {
            IsTerminalOpen = true;
            var dialog = new TerminalEditorWindow(this);
            dialog.ShowDialog();
            IsTerminalOpen = false;

            if (currentLevel == 3 && activationCode.Trim() == "DEBUG")
            {
                if (exitDoorLocked)
                {
                    SetExitDoorLocked(false);
                    SaveGame(); // 💾 Сохраняем сразу после открытия двери на 3 уровне
                    MessageBox.Show("Код активирован! Выход открыт.", "Успех");
                }
            }
            UpdateTerminalImage();
        }
    }
}
