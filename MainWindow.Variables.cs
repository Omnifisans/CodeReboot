namespace CodeRebootWPF
{
    public partial class MainWindow
    {
        internal bool GetExitDoorLocked() => exitDoorLocked;
        internal void SetExitDoorLocked(bool value) { exitDoorLocked = value; CheckAndOpenDoor(); }
        internal int GetPowerLevel() => powerLevel;
        internal void SetPowerLevel(int value) => powerLevel = value;
        internal bool GetGeneratorActive() => generatorActive;
        internal void SetGeneratorActive(bool value) => generatorActive = value;
        internal int GetDoorCode() => doorCode;
        internal void SetDoorCode(int value) { doorCode = value; CheckAndOpenDoor(); }
        internal string GetActivationCode() => activationCode;
        internal void SetActivationCode(string value)
        {
            activationCode = value;
            if (currentLevel == 3 && activationCode.Trim() == "DEBUG")
            {
                SetExitDoorLocked(false);
            }
            else
            {
                CheckAndOpenDoor();
            }
        }
        internal int GetCurrentLevel() => currentLevel;
        internal bool GetHasPiece1() => hasPiece1;
        internal bool GetHasPiece2() => hasPiece2;
        internal bool GetDoorSpawned() => doorSpawned;

        private void CheckAndOpenDoor()
        {
            if (currentLevel == 1 && !exitDoorLocked) OpenDoor();
            else if (currentLevel == 2 && !exitDoorLocked && doorCode == 42) OpenDoor();
            else if (currentLevel == 3 && !exitDoorLocked) OpenDoor();
            else if (currentLevel == 4 && !exitDoorLocked) OpenDoor();
            else if (currentLevel == 5 && !exitDoorLocked && doorSpawned) OpenDoor();
        }
    }
}
