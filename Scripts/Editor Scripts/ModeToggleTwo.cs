using UnityEngine;
using System.Collections;

public class ModeToggleTwo : MonoBehaviour
{
    [SerializeField] KeyCode switchKey = KeyCode.Tab;
    [SerializeField] KeyCode submodeKey = KeyCode.Z;
    IEditorMode[] modes;

    int currentActiveMode = 0;

    // Use this for initialization
    void Start() {
        modes = GetComponents<IEditorMode>();
        DisableModes();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(switchKey))
            SwitchMode();
        if (Input.GetKeyDown(submodeKey))
            SwitchSubMode();
    }

    void SwitchMode() {
        DisableModes();
        currentActiveMode = (currentActiveMode + 1) % modes.Length;
        modes[currentActiveMode].ActivateMode(true);
    }

    void DisableModes() {
        foreach (IEditorMode mode in modes) {
            mode.ActivateMode(false);
        }
    }

    void SwitchSubMode() {
        modes[currentActiveMode].RotateSubMode();
    }
}
