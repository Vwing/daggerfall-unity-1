using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInjector : MonoBehaviour {
    public GameObject SteamVRPrefab;
    public GameObject CameraRigPrefab;
    public GameObject UnderControllerUIPrefabLeft;
    public GameObject UnderControllerUIPrefabRight;
    public GameObject VRUIManagerPrefab;
    public GameObject OverControllerUIPrefab;

    [Tooltip("This name must match the name in the left controller prefab")]
    public String controllerLeftName = "Controller (left)";

    [Tooltip("This name must match the name in the right controller prefab")]
    public String controllerRightName = "Controller (right)";
    
    [Tooltip("The name assigned by SteamVR when it creates the camera object for the player's eyes")]
    public String cameraEyeName = "Camera (eye)";

    [Tooltip("The name in Daggerfall Unity for the PlayerAdvanced GameObject")]
    public String playerAdvancedName = "PlayerAdvanced";

    // TODO: Change this value based on user's height?
    [Tooltip("Changes the player's height to this value. Currently this is determined by testing, but maybe it should be scaled based on " +
        "the VR user's actual height.")]
    public float defaultCharacterControllerHeight = 0.8f;

    private GameObject player;
    private PlayerMouseLook playerMouseLook;
    private GameObject steamVR;
    private GameObject cameraRig;
    private GameObject controllerLeft;
    private GameObject controllerRight;
    private GameObject oldCamera;
    private GameObject eyesCamera;
    private GameObject vruiManager;
    private GameObject playerAdvanced;

    private void Start() {
        StartCoroutine(Setup());
    }

    private IEnumerator Setup() {
        yield return new WaitForSeconds(1); // the game starts paused. When unpaused, one second after start up it'll inject

        if (!SteamVRPrefab || !CameraRigPrefab || !UnderControllerUIPrefabLeft || !UnderControllerUIPrefabRight || !VRUIManagerPrefab || !OverControllerUIPrefab) { 
            Debug.LogError("Attempted to inject VR, but one or more of the default prefabs aren't set! SteamVRPrefab, CameraRigPrefabLeft/Right, UnderControllerUIPrefab, VRUIManagerPrefab, or OverControllerUIPrefab. This error is non-recoverable for VR support.");
            yield return 0;
        }

        player = GameObject.Find("PlayerAdvanced");
        playerMouseLook = player.GetComponentInChildren<PlayerMouseLook>();
        if (!player || !playerMouseLook) {
            Debug.LogError("Attempted to inject VR but I wasn't able to find either the PlayerAdvanced or the PlayerMouseLook! This error is non-recoverable for VR support.");
            yield return 0;
        }

        try {
            oldCamera = player.transform.Find("SmoothFollower").gameObject.transform.Find("Camera").gameObject;
            oldCamera.GetComponent<Camera>().enabled = false;
            oldCamera.GetComponent<AudioListener>().enabled = false;
        }
        catch (Exception) {
            Debug.LogError("Unable to get the original camera and/or the old AudioListenerer to disable it for VR! If you continue, VR support will most likely be broken.");
        }

        steamVR = GameObject.Instantiate(SteamVRPrefab);
        cameraRig = GameObject.Instantiate(CameraRigPrefab);
        cameraRig.transform.SetParent(player.transform);
        cameraRig.transform.position = player.transform.position;
        playerMouseLook.enabled = false;

        controllerRight = cameraRig.transform.Find(controllerRightName).gameObject;
        controllerLeft = cameraRig.transform.Find(controllerLeftName).gameObject;

        if (controllerLeft && controllerRight) {
            GameObject controller = GameObject.Instantiate(UnderControllerUIPrefabLeft);
            controller.transform.parent = controllerLeft.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);
            controller.GetComponent<UnderHandUIController>().myController = controllerLeft;

            controller = GameObject.Instantiate(UnderControllerUIPrefabRight);
            controller.transform.parent = controllerRight.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);
            controller.GetComponent<UnderHandUIController>().myController = controllerRight;

            controller = GameObject.Instantiate(OverControllerUIPrefab);
            controller.transform.parent = controllerLeft.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);

            controller = GameObject.Instantiate(OverControllerUIPrefab);
            controller.transform.parent = controllerRight.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);
        }
        else {
            Debug.LogError("Unable to get the two VR controller objects! If you continue the UI for VR controllers will be broken.");
        }

        eyesCamera = GameObject.Find(cameraEyeName);
        if (eyesCamera && oldCamera) {
            SphereCollider uiHeadCollider = eyesCamera.AddComponent<SphereCollider>();
            uiHeadCollider.isTrigger = true;
            uiHeadCollider.radius = 0.3f;
            oldCamera.transform.localPosition = new Vector3(0, 0, 0);
            oldCamera.transform.parent = eyesCamera.transform;
            oldCamera.transform.rotation = Quaternion.identity;
        }
        else {
            Debug.LogError("Unable to get the newly created 'Camera (eyes)' object! If you continue, the VR UI and sprite rotation will be broken.");
        }

        vruiManager = GameObject.Instantiate(VRUIManagerPrefab);

        playerAdvanced = GameObject.Find(playerAdvancedName);
        CharacterController cc = null;
        if (playerAdvanced) {
            cc = playerAdvanced.GetComponent<CharacterController>();
            if (cc) {
                cc.height = defaultCharacterControllerHeight;
            } else {
                Debug.LogError("Got the PlayerAdvanced GameObject, but it didn't seem to contain a CharacterController! Player height may be wrong.");
            }

            PlayerActivate pa = playerAdvanced.GetComponent<PlayerActivate>();
            if (pa) {
                pa.mainCamera = eyesCamera;
            } else {
                Debug.LogError("Got the PlayerAdvanced GameObject, but it didn't seem to contain a PlayerAdvanced script! Activating things in VR will be broken.");
            }

        } else {
            Debug.LogError("Unable to get the PlayerAdvanced (" + playerAdvancedName + ") GameObject! Wrong name set in VRInjector in Unity Editor? Player height and some other things may be incorrect.");
        }

        //TODO: DEBUG: REMOVEME:
        playerAdvanced.transform.position = new Vector3(18.60975f, 0.46f, 18.25393f);
        playerAdvanced.transform.eulerAngles = new Vector3(0, 155.087f, 0);


    }
	
	void Update() {
		
	}
}
