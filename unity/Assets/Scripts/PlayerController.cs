using UnityEngine;

namespace CriminalStory
{
    /// <summary>
    /// First-person rig: a CharacterController with a camera child.
    /// Story-driven design: the controller can be fully disabled (Chapter 0,
    /// dialogue, end panel) so the story, not free movement, leads.
    /// Eye height is adjustable so Chapter 0 (newborn, lying down) and
    /// Chapter 1 (small child) can use a lower viewpoint than an adult.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public bool ControlEnabled { get; set; }

        public Camera Cam { get; private set; }

        private CharacterController _controller;
        private float _yaw;
        private float _pitch;
        private float _verticalVelocity;
        private const float WalkSpeed = 3.4f;
        private const float LookSensitivity = 2.0f;

        public void BuildRig()
        {
            _controller = gameObject.AddComponent<CharacterController>();
            _controller.height = 1.1f;   // small child (Chapter 1)
            _controller.radius = 0.3f;
            _controller.center = new Vector3(0f, 0.55f, 0f);

            GameObject camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            Cam = camGo.AddComponent<Camera>();
            Cam.nearClipPlane = 0.05f;
            Cam.farClipPlane = 400f;
            Cam.fieldOfView = 68f;
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";

            ControlEnabled = false;
        }

        /// <summary>Places the rig and faces it along yaw (degrees, Y axis).</summary>
        public void Teleport(Vector3 position, float yawDegrees)
        {
            if (_controller != null)
            {
                _controller.enabled = false;
            }
            transform.position = position;
            _yaw = yawDegrees;
            _pitch = 0f;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (Cam != null)
            {
                Cam.transform.localRotation = Quaternion.identity;
            }
            if (_controller != null)
            {
                _controller.enabled = true;
            }
        }

        /// <summary>Sets controller height + camera height (child vs adult).</summary>
        public void SetEyeHeight(float controllerHeight, float cameraHeight)
        {
            if (_controller != null)
            {
                _controller.height = controllerHeight;
                _controller.center = new Vector3(0f, controllerHeight * 0.5f, 0f);
            }
            if (Cam != null)
            {
                Vector3 p = Cam.transform.localPosition;
                p.y = cameraHeight;
                Cam.transform.localPosition = p;
            }
        }

        public void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void Update()
        {
            if (!ControlEnabled || _controller == null || !_controller.enabled)
            {
                return;
            }

            // Mouse look (only while the cursor is captured).
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                _yaw += Input.GetAxis("Mouse X") * LookSensitivity;
                _pitch -= Input.GetAxis("Mouse Y") * LookSensitivity;
                _pitch = Mathf.Clamp(_pitch, -80f, 80f);
            }
            // Arrow keys also turn, for trackpads / accessibility.
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _yaw -= 90f * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _yaw += 90f * Time.deltaTime;
            }

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (Cam != null)
            {
                Cam.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }

            float forward = 0f;
            float strafe = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                forward += 1f;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                forward -= 1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                strafe -= 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                strafe += 1f;
            }

            Vector3 move = transform.forward * forward + transform.right * strafe;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }
            move *= WalkSpeed;

            if (_controller.isGrounded)
            {
                _verticalVelocity = -1.0f;
            }
            else
            {
                _verticalVelocity -= 18f * Time.deltaTime;
            }
            move.y = _verticalVelocity;

            _controller.Move(move * Time.deltaTime);
        }
    }
}
