using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private Camera cam;
    [SerializeField]
    private float distance = 3f;
    [SerializeField]
    private LayerMask mask;

    private PlayerUI playerUI;
    private InputManager inputManager;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        playerUI.UpdateText(string.Empty);
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);
        RaycastHit hitinfo;
        if(Physics.Raycast(ray, out hitinfo, distance, mask))
        {
            if(hitinfo.collider.GetComponent<Interactuable>() != null)
            {
                Interactuable interactuable = hitinfo.collider.GetComponent<Interactuable>();
                playerUI.UpdateText(interactuable.promptMessage);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactuable.BaseInteract();
                }
            }
        }
    }
}
