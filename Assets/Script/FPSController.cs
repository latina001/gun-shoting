using UnityEngine;
using System.Collections;
using TMPro; // 🔥 ใช้ตัวนี้แทน UI.Text

public class FPSController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cameraPivot;
    public Transform cameraHolder;
    public Animator anim;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float gravity = -9.8f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 200f;
    float xRotation = 0f;

    [Header("Gun")]
    public int maxAmmo = 30;
    int currentAmmo;

    public float fireRate = 0.2f;
    float nextFireTime = 0f;

    public float reloadTime = 1.5f;
    bool isReloading = false;

    [Header("Recoil")]
    public float recoilAmount = 2f;
    public float recoilRecoverySpeed = 5f;
    float recoilX;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float crouchCameraY = 1f;
    float normalHeight;
    float normalCameraY;
    bool isCrouching = false;

    [Header("Zoom")]
    public float normalFOV = 60f;
    public float zoomFOV = 40f;
    public float zoomSpeed = 10f;

    public float normalSensitivity = 200f;
    public float zoomSensitivity = 100f;

    [Header("UI")]
    public TMP_Text ammoText; // 🔥 เปลี่ยนตรงนี้

    Camera cam;
    float yVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentAmmo = maxAmmo;

        normalHeight = controller.height;
        normalCameraY = cameraHolder.localPosition.y;

        cam = Camera.main;

        if (anim == null)
            Debug.LogError("❌ Animator not assigned!");
    }

    void Update()
    {
        if (!controller.enabled)
        {
            anim.SetFloat("Speed", 0f);
            return;
        }

        Look();
        Move();
        Zoom();

        if (isReloading) return;

        Shoot();

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());

        if (Input.GetKeyDown(KeyCode.C))
            ToggleCrouch();

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        // 🔫 UI
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }

    void Look()
    {
        float currentSensitivity = Input.GetMouseButton(1) ? zoomSensitivity : normalSensitivity;

        float mouseX = Input.GetAxis("Mouse X") * currentSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * currentSensitivity * Time.deltaTime;

        recoilX = Mathf.Lerp(recoilX, 0f, Time.deltaTime * recoilRecoverySpeed);

        xRotation -= mouseY;
        xRotation -= recoilX;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (isCrouching) speed *= 0.5f;

        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);

        float speedPercent = move.magnitude;
        if (speedPercent < 0.1f) speedPercent = 0f;

        anim.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
        anim.SetBool("isRunning", speed == runSpeed);
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            if (Input.GetButtonDown("Fire1"))
                anim.SetTrigger("Empty");
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;

            anim.SetTrigger("Shoot");
            recoilX += recoilAmount;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    Enemy enemy = hit.transform.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(20);
                    }
                }
            }
        }
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        anim.SetTrigger("Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void Jump()
    {
        if (!controller.isGrounded) return;

        yVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        anim.SetTrigger("Jump");
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            controller.height = crouchHeight;
            StartCoroutine(SmoothCrouch(crouchCameraY));
        }
        else
        {
            controller.height = normalHeight;
            StartCoroutine(SmoothCrouch(normalCameraY));
        }

        controller.center = new Vector3(0, controller.height / 2f, 0);
    }

    IEnumerator SmoothCrouch(float targetY)
    {
        float startY = cameraHolder.localPosition.y;
        float time = 0f;

        while (time < 0.2f)
        {
            float y = Mathf.Lerp(startY, targetY, time / 0.2f);
            cameraHolder.localPosition = new Vector3(0, y, 0);

            time += Time.deltaTime;
            yield return null;
        }

        cameraHolder.localPosition = new Vector3(0, targetY, 0);
    }

    void Zoom()
    {
        bool isAimingNow = Input.GetMouseButton(1);

        if (isAimingNow)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomFOV, Time.deltaTime * zoomSpeed);
        else
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalFOV, Time.deltaTime * zoomSpeed);

        anim.SetBool("isAiming", isAimingNow);
    }
}