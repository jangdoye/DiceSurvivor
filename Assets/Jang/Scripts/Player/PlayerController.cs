using DiceSurvivor.Audio;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceSurvivor.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Variables
        [Header("이동")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSmoothTime = 0.1f;

        [Header("중력/그라운드")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.3f;
        [SerializeField] private LayerMask groundMask;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float turnSmoothVelocity;
        private Vector2 inputMove;
        private Camera mainCamera;

        [Header("버프/자석")]
        /*[SerializeField] private float normalSpeed = 5f;
        [SerializeField] private float buffedSpeed = 8f;*/
        private float cachedSpeed;
        private Coroutine buffCoroutine;
        private Coroutine magnetCoroutine;

        // 체력은 PlayerHealth가 관리
        private PlayerHealth health;

        [SerializeField] private float extraPadding = 0.1f;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            if (controller == null)
            {
                Debug.LogError("CharacterController가 없습니다!");
                enabled = false;
                return;
            }
            if (health == null)
            {
                Debug.LogError("PlayerHealth가 없습니다!");
                enabled = false;
                return;
            }

            mainCamera = Camera.main;

            controller.stepOffset = 0.05f; // 기본(0.3~0.5)보다 훨씬 낮게
            controller.slopeLimit = 45f;   // 과도한 경사 타기 방지(프로젝트에 맞게)
            controller.skinWidth = 0.03f;  // 너무 크면 끼임/튐. 너무 낮아도 관통주의
        }

        private void Update()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
                return;
            if (health != null && health.IsDead) return;

            GroundCheck();
            Move();
            ApplyGravity();
        }
        #endregion

        #region Movement
        private void GroundCheck()
        {
            if (groundCheck == null)
            {
                //Debug.LogWarning("GroundCheck가 설정되지 않았습니다.");
                return;
            }

            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0f) velocity.y = -2f;
        }

        private void Move()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy) return;

            Vector3 direction = new Vector3(inputMove.x, 0f, inputMove.y).normalized;
            if (direction.magnitude >= 0.1f)
            {
                float camY = (mainCamera != null) ? mainCamera.transform.eulerAngles.y : 0f;
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camY;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            inputMove = context.ReadValue<Vector2>();
        }

        private void ApplyGravity()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy) return;

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        #endregion

        #region Interaction / Effects
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.TryGetComponent(out Box box))
                box.BreakOnTouch();
        }

        public void TakeDamage(float damage)
        {
            if (health == null) return;
            health.TakeDamage(damage, true); // 무적/깜빡임 포함
        }

        public void Heal(float amount)
        {
            if (health == null) return;
            health.Heal(amount);
        }

        public void AddCoins(int amount)
        {
            GoldWallet.Instance.Add(amount);
        }

        public void ApplySpeedBuff(float multiplier, float duration)
        {
            if (buffCoroutine != null) StopCoroutine(buffCoroutine);
            buffCoroutine = StartCoroutine(SpeedBuffRoutine(multiplier, duration));
        }

        private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
        {
            cachedSpeed = moveSpeed;
            moveSpeed *= multiplier;
            //Debug.Log($"이동속도 버프 시작: {moveSpeed}");
            yield return new WaitForSeconds(duration);
            moveSpeed = cachedSpeed;
            //Debug.Log("이동속도 버프 종료");
        }

        public void ActivateMagnet(float duration)
        {
            if (magnetCoroutine != null) StopCoroutine(magnetCoroutine);
            magnetCoroutine = StartCoroutine(MagnetRoutine(duration));
        }

        private IEnumerator MagnetRoutine(float duration)
        {
            //Debug.Log("자석 효과 시작");
            // TODO: 자석 범위 내 코인 끌어당기기 로직
            yield return new WaitForSeconds(duration);
            //Debug.Log("자석 효과 종료");
        }

        #endregion
    }
}