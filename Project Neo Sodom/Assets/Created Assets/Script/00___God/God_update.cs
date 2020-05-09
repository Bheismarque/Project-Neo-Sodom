using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class God_update : MonoBehaviour
{
    private void Awake()
    {
        // God Setup
        God.god_update = gameObject;

        // System Setup
        sys_Interactable.systemSetUp();
        UIS.systemSetUp();

        // Input System Setup
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Game Componenet Setup
        God.CAMERA = transform.GetChild(0).GetComponent<sys_Camera_ShoulderView>();
    }
    
    private void Update()
    {
        God.input.inputCheck();
        God.gameTime = God.timeRatio * Time.deltaTime;
        God.deltaTime = Time.deltaTime;

        if (Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.T))
        {
            God.NPCs = new List<CharacterHandler>();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
