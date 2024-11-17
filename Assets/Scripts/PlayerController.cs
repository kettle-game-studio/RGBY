using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float mouseSpeed = 10;

    public Vector2 rotationLimits = new Vector2(-60, 60);
    public Transform cameraCenter;
    public JumpCollider jumpCollider;
    public MeshRenderer mesh;
    public RectTransform circleContainer;

    public ControllerSettings[] controllers;
    public IController[] controllerInstances;
    public RawImage[] circleParts;


    InputAction lookAction;
    InputAction attackAction;
    Rigidbody body;

    Vector2 lookRotation;

    Vector3 restartLocation;
    Vector3 restartRotation;
    Vector2 mouseCenterPosition;

    ControlState state = new ControlState();
    int lastSegment;
    int currentSegment = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        lookAction   = InputSystem.actions.FindAction("Look");
        attackAction = InputSystem.actions.FindAction("Attack");

        lookRotation = new Vector2(cameraCenter.transform.rotation.eulerAngles.y, cameraCenter.transform.rotation.eulerAngles.x);
        body = GetComponent<Rigidbody>();

        restartLocation = lookRotation;
        restartRotation = transform.position;


        controllerInstances = controllers.Select(c => c.Construct()).ToArray();
        circleParts = circleContainer.Cast<RectTransform>().Select(c => c.GetComponent<RawImage>()).ToArray();

        state.jumper = jumpCollider;
        state.transform = body.transform;

        controllerInstances[currentSegment].Enabled = true;
        mesh.material.SetColor("_BaseColor", circleParts[currentSegment].color);
    }

    void Update()
    {
        Vector2 lookValue = lookAction.ReadValue<Vector2>();

        lookRotation += lookValue * Time.deltaTime * mouseSpeed;
        lookRotation = new Vector2(lookRotation.x,
            Mathf.Max(Mathf.Min(lookRotation.y, rotationLimits.y), rotationLimits.x));

        cameraCenter.transform.rotation = Quaternion.Euler(lookRotation.y, lookRotation.x, 0);


        if (attackAction.IsPressed())
        {
            circleContainer.gameObject.SetActive(true);
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                mouseCenterPosition = Mouse.current.position.ReadValue();
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Vector2 delta = Mouse.current.position.ReadValue() - mouseCenterPosition;
                if (delta.magnitude < 15) lastSegment = 0;
                else if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && delta.x < 0) lastSegment = 1;
                else if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && delta.x > 0) lastSegment = 2;
                else if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y) && delta.y < 0) lastSegment = 4;
                else if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y) && delta.y > 0) lastSegment = 3;
                for (int i = 0; i < circleParts.Count(); ++i)
                    circleParts[i].transform.localScale = new Vector3(1, 1, 1) * (i == lastSegment ? 1.3f : 1f);
            }
        }
        else
        {
            circleContainer.gameObject.SetActive(false);
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                controllerInstances[currentSegment].Enabled = false;
                currentSegment = lastSegment;
                controllerInstances[currentSegment].Enabled = true;
                mesh.material.SetColor("_BaseColor", circleParts[currentSegment].color);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    void FixedUpdate()
    {
        state.forwardRotation = Quaternion.Euler(0, lookRotation.x, 0);
        state.lookRotation = cameraCenter.transform.rotation;

        state.velocity = new Vector3(0, body.linearVelocity.y, 0);
        state.gravity = true;

        foreach (var controller in controllerInstances)
            controller.Iterate(state);

        body.linearVelocity = state.velocity;
        body.useGravity = state.gravity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Obstacle")
        {
            transform.position = restartLocation;
            lookRotation = restartRotation;
            body.linearVelocity = Vector3.zero;
        }
    }
}
