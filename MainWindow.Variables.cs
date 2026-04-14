namespace CodeRebootWPF
{
    public partial class MainWindow
    {
        internal bool GetExitDoorLocked() => exitDoorLocked;
        internal void SetExitDoorLocked(bool value)
        {
            exitDoorLocked = value;
            CheckAndOpenDoor();
        }

        internal int GetPowerLevel() => powerLevel;
        internal void SetPowerLevel(int value) => powerLevel = value;

        internal bool GetGeneratorActive() => generatorActive;
        internal void SetGeneratorActive(bool value) => generatorActive = value;

        internal int GetDoorCode() => doorCode;
        internal void SetDoorCode(int value)
        {
            doorCode = value;
            CheckAndOpenDoor();
        }

        internal int GetCurrentLevel() => currentLevel;

        private void CheckAndOpenDoor()
        {
            if (currentLevel == 1)
            {
                // На первом уровне дверь открывается只要 exitDoorLocked == false
                if (!exitDoorLocked)
                    OpenDoor();
            }
            else if (currentLevel == 2)
            {
                // На втором уровне нужно оба условия
                if (!exitDoorLocked && doorCode == 42)
                    OpenDoor();
            }
        }
    }
}