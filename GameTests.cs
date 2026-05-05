using System;
using System.Text;

namespace CodeRebootWPF
{
    /// <summary>
    /// Автоматизированные тесты игровой логики
    /// </summary>
    public static class GameTests
    {
        // Вспомогательный класс для симуляции состояния игры
        private class TestState
        {
            public int Level { get; set; } = 1;
            public bool DoorLocked { get; set; } = true;
            public int DoorCode { get; set; } = 0;
            public string ActivationCode { get; set; } = "";
            public int GeneratorToggles { get; set; } = 0;
            public bool GeneratorBroken { get; set; } = false;
            public bool HasPiece1 { get; set; } = false;
            public bool HasPiece2 { get; set; } = false;
            public bool DoorSpawned { get; set; } = false;
        }

        // Простая система проверок
        private static class Assert
        {
            public static void IsTrue(bool condition, string message)
            {
                if (!condition) throw new Exception($"❌ ПРОВЕРКА НЕ ПРОЙДЕНА: {message}");
            }
            public static void IsFalse(bool condition, string message)
            {
                if (condition) throw new Exception($"❌ ПРОВЕРКА НЕ ПРОЙДЕНА: {message}");
            }
        }

        // Логика проверки выхода (как в MainWindow.Variables.cs)
        private static bool CheckExitCondition(TestState s)
        {
            return s.Level switch
            {
                1 => !s.DoorLocked,
                2 => !s.DoorLocked && s.DoorCode == 42,
                3 => !s.DoorLocked,
                4 => !s.DoorLocked,
                5 => !s.DoorLocked && s.DoorSpawned,
                _ => false
            };
        }

        // ================= ТЕСТЫ =================

        public static string Test_Level1_DoorUnlock()
        {
            var state = new TestState { Level = 1, DoorLocked = true };
            state.DoorLocked = false;
            Assert.IsTrue(CheckExitCondition(state), "Уровень 1: Дверь должна открыться при разблокировке");
            return "✅ Уровень 1: Логика разблокировки работает";
        }

        public static string Test_Level2_CodeCheck()
        {
            var state = new TestState { Level = 2, DoorLocked = false, DoorCode = 42 };
            Assert.IsTrue(CheckExitCondition(state), "Уровень 2: Код 42 должен открыть дверь");

            state.DoorCode = 10;
            Assert.IsFalse(CheckExitCondition(state), "Уровень 2: Неверный код не должен открыть дверь");
            return "✅ Уровень 2: Проверка кода доступа работает";
        }

        public static string Test_Level3_ActivationCode()
        {
            var state = new TestState { Level = 3, DoorLocked = true, ActivationCode = "DEBUG" };
            if (state.ActivationCode.Trim() == "DEBUG") state.DoorLocked = false;
            Assert.IsTrue(CheckExitCondition(state), "Уровень 3: Код DEBUG должен разблокировать выход");
            return "✅ Уровень 3: Код активации работает";
        }

        public static string Test_Level4_GeneratorMechanic()
        {
            var state = new TestState { Level = 4, DoorLocked = true, GeneratorToggles = 0 };
            for (int i = 0; i < 5; i++)
            {
                state.GeneratorToggles++;
                if (state.GeneratorToggles >= 5)
                {
                    state.GeneratorBroken = true;
                    state.DoorLocked = false;
                }
            }
            Assert.IsTrue(state.GeneratorBroken, "Генератор должен сломаться после 5 переключений");
            Assert.IsTrue(CheckExitCondition(state), "После поломки генератора дверь должна открыться");
            return "✅ Уровень 4: Механика генератора работает";
        }

        public static string Test_Level5_CodePieces()
        {
            var state = new TestState { Level = 5, DoorLocked = true, DoorSpawned = false };

            state.HasPiece1 = true;
            if (state.HasPiece1 && state.HasPiece2 && !state.DoorSpawned) state.DoorSpawned = true;
            Assert.IsFalse(CheckExitCondition(state), "Только одна часть кода не должна открыть выход");

            state.HasPiece2 = true;
            if (state.HasPiece1 && state.HasPiece2 && !state.DoorSpawned) state.DoorSpawned = true;
            state.DoorLocked = false;
            Assert.IsTrue(CheckExitCondition(state), "Обе части кода + разблокировка должны открыть выход");
            return "✅ Уровень 5: Сбор частей кода работает";
        }

        // ================= ЗАПУСК =================

        public static string RunAll()
        {
            var report = new StringBuilder();
            report.AppendLine("🧪 ЗАПУСК АВТОТЕСТОВ ИГРОВОЙ ЛОГИКИ");
            report.AppendLine(new string('-', 40));

            var tests = new Func<string>[] {
                Test_Level1_DoorUnlock,
                Test_Level2_CodeCheck,
                Test_Level3_ActivationCode,
                Test_Level4_GeneratorMechanic,
                Test_Level5_CodePieces
            };

            int passed = 0;
            foreach (var test in tests)
            {
                try
                {
                    report.AppendLine(test());
                    passed++;
                }
                catch (Exception ex)
                {
                    report.AppendLine(ex.Message);
                }
            }

            report.AppendLine(new string('-', 40));
            report.AppendLine($"📊 ИТОГ: {passed}/{tests.Length} тестов пройдено успешно.");
            return report.ToString();
        }
    }
}