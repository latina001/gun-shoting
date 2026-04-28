using UnityEngine;
using System.Collections;

public class FPSController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraPivot;

    Animator anim;

    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float mouseSensitivity = 200f;
    public float gravity = -9.8f;

    [Header("Gun")]
    public int maxAmmo = 30;
    int currentAmmo;

    public float fireRate = 0.2f;
    float nextFireTime = 0f;

    public float reloadTime = 1.5f;
    bool isReloading = false;

    float yVelocity;
    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        anim = GetComponentInChildren<Animator>();
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        Look();
        Move();

        if (isReloading) return;

        Shoot();

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);

        // 🎬 Animation
        float speedPercent = move.magnitude;
        anim.SetFloat("Speed", speedPercent);
        anim.SetBool("isRunning", speed == runSpeed);
    }

    // 🔫 ยิง
    void Shoot()
    {
        if (currentAmmo <= 0) return;

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;

            anim.SetTrigger("Shoot");

            RaycastHit hit;
            if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out hit, 100f))
            {
                Debug.Log("Hit: " + hit.transform.name);
            }
        }
    }

    // 🔄 รีโหลด
    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;

        anim.SetTrigger("Reload"); // 🎬 เล่น animation

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }
}