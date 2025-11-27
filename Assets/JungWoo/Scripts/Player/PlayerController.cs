using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace DiceSurvivor.PlayerStats
{
    public class PlayerControllerer : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSmoothTime = 0.1f;

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

        [SerializeField] private float playerHP = 100f;
        //[SerializeField] private float normalSpeed = 5f;
        //[SerializeField] private float buffedSpeed = 8f;
        private float currentSpeed;

        private Coroutine buffCoroutine;
        private Coroutine magnetCoroutine;

        private bool isDead = false;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (controller == null)
            {
                Debug.LogError("CharacterController가 없습니다!");
                enabled = false;
                return;
            }

            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
                return;

            GroundCheck();
            Move();
            ApplyGravity();

            if (!isDead && playerHP <= 0f)
            {
                isDead = true;
                Die();
            }
        }
        #endregion

        #region Custom Method
        private void GroundCheck()
        {
            if (groundCheck == null)
            {
                Debug.LogWarning("GroundCheck가 설정되지 않았습니다.");
                return;
            }

            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0f)
                velocity.y = -2f;
        }

        private void Move()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
                return;

            Vector3 direction = new Vector3(inputMove.x, 0f, inputMove.y).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
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
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
            {
                Debug.LogWarning("ApplyGravity: controller 비활성 상태로 Move 중단");
                return;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.TryGetComponent(out Box box))
                box.BreakOnTouch();
        }

        public void Heal(float amount)
        {
            playerHP += amount;
            playerHP = Mathf.Min(playerHP, 100f);
            //Debug.Log($"HP 회복: {amount} → 현재 HP: {playerHP}");
        }

        public void AddCoins(int amount)
        {
            GoldWallet.Instance.Add(amount);
        }

        public void ApplySpeedBuff(float multiplier, float duration)
        {
            if (buffCoroutine != null)
                StopCoroutine(buffCoroutine);

            buffCoroutine = StartCoroutine(SpeedBuffRoutine(multiplier, duration));
        }

        private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
        {
            currentSpeed = moveSpeed;
            moveSpeed *= multiplier;
            //Debug.Log($"이동속도 버프 시작: {moveSpeed}");

            yield return new WaitForSeconds(duration);

            moveSpeed = currentSpeed;
            //Debug.Log("이동속도 버프 종료");
        }

        public void ActivateMagnet(float duration)
        {
            if (magnetCoroutine != null)
                StopCoroutine(magnetCoroutine);

            magnetCoroutine = StartCoroutine(MagnetRoutine(duration));
        }

        private IEnumerator MagnetRoutine(float duration)
        {
            //Debug.Log("자석 효과 시작");
            // 자석 범위 내 코인 끌어당기기 구현 가능
            yield return new WaitForSeconds(duration);
            //Debug.Log("자석 효과 종료");
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            playerHP -= damage;
            //Debug.Log("플레이어 HP: " + playerHP);
        }

        private void Die()
        {
            //Debug.Log("플레이어 사망");
            Time.timeScale = 0f;

            /*GameOverUI gameOver = FindObjectOfType<GameOverUI>();
            if (gameOver != null)
            {
                gameOver.ShowGameOverUI();
            }*/
        }
        #endregion
    }
}
