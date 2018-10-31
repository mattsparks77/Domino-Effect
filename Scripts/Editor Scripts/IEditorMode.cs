public interface IEditorMode {
    void ActivateMode(bool isActive);
    void RotateSubMode();
    string ModeName();
}
