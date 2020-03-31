using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class God_update : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        God.god_update = gameObject;
        God.CAMERA = transform.GetChild(0).GetComponent<sys_Camera_ShoulderView>();

        sys_Interactable.resetInteractableList();
    }
    
    private void Update()
    {
        God.input.inputCheck();
        God.gameTime = God.timeRatio * Time.deltaTime;
        God.deltaTime = Time.deltaTime;

        if (Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.T))
        {
            God.NPCs = new List<scr_PersonController>();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
