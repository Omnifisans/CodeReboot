using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CodeRebootWPF
{
    public class TerminalEditorWindow : Window
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

            int level = mainWindow.GetCurrentLevel();
            variables = new List<VariableInfo>();
            if (level == 1)
                variables.Add(new VariableInfo { Name = "exitDoorLocked", Type = "bool" });
            else if (level == 2)
            {
                variables.Add(new VariableInfo { Name = "exitDoorLocked", Type = "bool" });
                variables.Add(new VariableInfo { Name = "doorCode", Type = "int" });
            }
            else if (level == 3)
            {
                variables.Add(new VariableInfo { Name = "activationCode", Type = "string" });
            }
            else if (level == 4)
            {
                variables.Add(new VariableInfo { Name = "exitDoorLocked", Type = "bool" });
            }
            else if (level == 5)
            {
                if (mainWindow.GetHasPiece1() && mainWindow.GetHasPiece2())
                    variables.Add(new VariableInfo { Name = "exitDoorLocked", Type = "bool" });
                else
                    variables.Add(new VariableInfo { Name = "Сначала найдите части кода", Type = "none" });
            }

            panel = new StackPanel { Margin = new Thickness(10) };
            Content = panel;
            ShowVariableList();
        }

        private void ShowVariableList()
        {
            panel.Children.Clear();
            isEditing = false;
            panel.Children.Add(new TextBlock
            {
                Text = "Выберите переменную (↑↓ - перемещение, Enter - редактировать):",
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            });

            for (int i = 0; i < variables.Count; i++)
            {
                var varInfo = variables[i];
                object currentValue = null;
                if (varInfo.Name == "exitDoorLocked") currentValue = mainWindow.GetExitDoorLocked();
                else if (varInfo.Name == "doorCode") currentValue = mainWindow.GetDoorCode();
                else if (varInfo.Name == "activationCode") currentValue = mainWindow.GetActivationCode();
                else if (varInfo.Name == "Сначала найдите части кода") currentValue = "";

                var tb = new TextBlock
                {
                    Text = varInfo.Type == "none" ? varInfo.Name : $"{varInfo.Name} ({varInfo.Type}) = {currentValue}",
                    Margin = new Thickness(0, 2, 0, 2),
                    Padding = new Thickness(2)
                };
                if (i == selectedIndex)
                    tb.Background = Brushes.LightBlue;
                panel.Children.Add(tb);
            }

            var cancelButton = new Button { Content = "Отмена", Width = 80, Height = 30, Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            panel.Children.Add(cancelButton);
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
                else if (currentVarType == "string")
                {
                    string newValue = input.Trim();
                    mainWindow.SetActivationCode(newValue);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Эта переменная не редактируется", "Ошибка");
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!isEditing)
            {
                if (e.Key == Key.Up)
                {
                    selectedIndex = (selectedIndex - 1 + variables.Count) % variables.Count;
                    ShowVariableList();
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    selectedIndex = (selectedIndex + 1) % variables.Count;
                    ShowVariableList();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    var selected = variables[selectedIndex];
                    if (selected.Type == "none")
                    {
                        MessageBox.Show("Сначала выполните условия уровня", "Нет доступа");
                        return;
                    }
                    object val = null;
                    if (selected.Name == "exitDoorLocked") val = mainWindow.GetExitDoorLocked();
                    else if (selected.Name == "doorCode") val = mainWindow.GetDoorCode();
                    else if (selected.Name == "activationCode") val = mainWindow.GetActivationCode();
                    ShowEditPanel(selected.Name, selected.Type, val);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                    Close();
                    e.Handled = true;
                }
            }
            base.OnKeyDown(e);
        }

        private class VariableInfo { public string Name { get; set; } public string Type { get; set; } }
    }
}
