using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class FlyCamera : MonoBehaviour
{
    public EventSystem eventSystem;
    public bool lockXRotation = false;
    public float minSpeed = 0.5f;
    public float mainSpeed = 10f; // Regular speed.
    public float shiftMultiplier = 2f;  // Multiplied by how long shift is held.  Basically running.
    public float maxShift = 100000f; // Maximum speed when holding shift.
    public float camSens = .35f;  // Camera sensitivity by mouse input.
    private Vector3 lastMouse = new Vector3(Screen.width / 2, Screen.height / 2, 0); // Kind of in the middle of the screen, rather than at the top (play).
    private float totalRun = 1.0f;

    public bool clickToMove = true;
    public float unitsAboveTerrain = 4f;

    public bool isFrozen = false;

    public Transform camTransfor;
    void Start()
    {
        eventSystem = EventSystem.current;
    }


    void Update()
    {
        if (isFrozen)
        {
            return;
        }

        // This should prevent the mouse input from getting through the UI.
        //if (eventSystem.IsPointerOverGameObject() && !dragging)
        //    return;

        mainSpeed += Input.GetAxis("Mouse ScrollWheel") * mainSpeed;
        if (mainSpeed < minSpeed)
            mainSpeed = minSpeed;

        if (clickToMove)
        {
            if (!Input.GetMouseButton(0))
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                lastMouse = Input.mousePosition;
                return;
            }
        }

        // Mouse input.
        lastMouse = Input.mousePosition - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
        lastMouse = new Vector3(camTransfor.eulerAngles.x + lastMouse.x, camTransfor.eulerAngles.y + lastMouse.y, 0);
        camTransfor.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;


        // Keyboard commands.
            Vector3 p = getDirection();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //totalRun += Time.deltaTime;
            totalRun += Time.unscaledDeltaTime;
            p = p * totalRun * shiftMultiplier;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        //p = p * Time.deltaTime;
        p = p * Time.unscaledDeltaTime;
        Vector3 newPosition = camTransfor.position;
        if (Input.GetKey(KeyCode.V))
        { //If player wants to move on X and Z axis only
            camTransfor.Translate(p);
            newPosition.x = camTransfor.position.x;
            newPosition.z = camTransfor.position.z;
            camTransfor.position = newPosition;
        }
        else
        {
            camTransfor.Translate(p);
        }

        if (lockXRotation)
        {
            Vector3 euler = camTransfor.localRotation.eulerAngles;
            euler.x = 0f;
            camTransfor.localRotation = Quaternion.Euler(euler);
        }
    }

    private Vector3 getDirection()
    {
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.R))
        {
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.F))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        return p_Velocity;
    }

    public void resetRotation(Vector3 lookAt)
    {
        camTransfor.LookAt(lookAt);
    }
}