using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1;
    public float mouseSpeed = 10;

    public Vector2 rotationLimits = new Vector2(-60, 60);
    public Transform cameraCenter;

    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    Rigidbody body;

    Vector2 lookRotation;
    bool jumped = false;

    Vector3 restartLocation;
    Vector3 restartRotation;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
        lookRotation = new Vector2(cameraCenter.transform.rotation.eulerAngles.y, cameraCenter.transform.rotation.eulerAngles.x);
        body = GetComponent<Rigidbody>();

        restartLocation = lookRotation;
        restartRotation = transform.position;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        Vector2 lookValue = lookAction.ReadValue<Vector2>();

        lookRotation += lookValue * Time.deltaTime * mouseSpeed;
        lookRotation = new Vector2(lookRotation.x,
            Mathf.Max(Mathf.Min(lookRotation.y, rotationLimits.y), rotationLimits.x));

        cameraCenter.transform.rotation = Quaternion.Euler(lookRotation.y, lookRotation.x, 0);

        float upSpeed = body.linearVelocity.y;

        if (jumpAction.IsPressed())
        {
            if (!jumped)
            {
                jumped = true;
                upSpeed = 10;
            }
        } else
            jumped = false;

        body.linearVelocity = Quaternion.Euler(0, lookRotation.x, 0) * (
            Vector3.forward * moveValue.y +
            Vector3.right   * moveValue.x
        ) * moveSpeed +
        Vector3.up * upSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Obstacle")
        {
            transform.position = restartLocation;
            lookRotation = restartRotation;
            body.linearVelocity = new Vector3();
        }
    }
}
