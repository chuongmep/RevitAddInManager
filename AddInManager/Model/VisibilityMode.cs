namespace RevitAddinManager.Model
{
    [Flags]
    public enum VisibilityMode
    {
        AlwaysVisible = 0,
        NotVisibleInProject = 1,
        NotVisibleInFamily = 2,
        NotVisibleWhenNoActiveDocument = 4,
        NotVisibleInArchitecture = 8,
        NotVisibleInStructure = 16,
        NotVisibleInMechanical = 32,
        NotVisibleInElectrical = 64,
        NotVisibleInPlumbing = 128,
        NotVisibleInMEP = 224
    }

    public enum VisibleModel
    {
        Enable,
        Disable
    }
}
