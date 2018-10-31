using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour {

	[SerializeField] RectTransform rootMenu;
	[SerializeField] Text modeButtonText;
	[SerializeField] RectTransform joinMenu;
	[SerializeField] RectTransform hostMenu;

    [SerializeField] NetworkManager NetworkMgr;
    [SerializeField] InputField addressField;

    public void HostMenuButton()
    {
        NetworkMgr.StartHost();
        NetworkMgr.ServerChangeScene("Proto Grav Level");
    }

	public void ModeButton(){
		//toggles the additional gravity directions
		//DominoSpawner.fixedGravityMode = !DominoSpawner.fixedGravityMode;
		/*
		modeButtonText.text = "Mode:Sandbox";
		if (DominoSpawner.fixedGravityMode == false)
			modeButtonText.text = "Mode:Gravity";
			*/
	}

	public void JoinMenuButton(){
		HideAll();
		joinMenu.gameObject.SetActive(true);
	}

	public void ReturnToRoot(){
		HideAll();
		rootMenu.gameObject.SetActive(true);
	}

	public void ExitButton()
	{
		Application.Quit();
	}

	public void SetConnectionAddress(){
        NetworkMgr.networkAddress = addressField.text;
	}

	public void SetConnectionPort(int port){
		NetworkMgr.networkPort = port;
	}

	void TryHost(){
		NetworkManager.singleton.StartHost();
	}

    public void Connect()
    {
        NetworkMgr.StartClient();
    }

    void HideAll(){
		rootMenu.gameObject.SetActive(false);
		joinMenu.gameObject.SetActive(false);
		hostMenu.gameObject.SetActive(false);
	}
}
