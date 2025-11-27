using UnityEngine;
using System.Collections;

namespace DiceSurvivor.Enemy {
    public class BossActivity : EnemyActivity
    {
        //일반 적 이동에서 약간 변형해야 함
        #region Variables
        // 패턴 종료 후 다음 패턴까지 시간
        [SerializeField] float patternCooldown = 2f;
        float patternTimer = 0f;
        // 현재 패턴 진행 여부
        bool isPatternActive = false;
        //패턴 결정
        bool patternSwitch = false;
        //패턴에 사용할 요소
        //돌진 위험 효과
        [SerializeField] LineRenderer chargeEffect;
        float chargePrepare = 2f;
        //소환할 탄환 프리팹
        [SerializeField] GameObject bulletPrefab;
        float bulletCount = 6f;

        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        protected override void Start()
        {
            base.Start();
            // 보스 전용 초기화
            chargeEffect = GetComponent<LineRenderer>();
            chargeEffect.enabled = false;
        } 
        
        protected override void Update()
        {
            if (playerPos == null) return;
            if (!isPatternActive)
            {
                patternTimer += Time.deltaTime;
                if (patternTimer >= patternCooldown)
                {
                    patternTimer = 0f;
                    isPatternActive = true;
                    DoPattern();
                }
            }

        }
        #endregion

        #region Custom Methods
        void DoPattern()
        {
            patternSwitch = !patternSwitch;
            //패턴 1. 돌진 - 시전 시점의 플레이어 위치로 2초 후 돌진
            if (patternSwitch)
            {
                //StartCoroutine(ChargeAttack(2));
            }
            //패턴 2. 탄환 발사 - 커다란 탄환을 6개 발사, 탄환은 보스의 자식으로 지정된 후 5초 후 사망()
            else
            {
                //StartCoroutine(FireBullets(6));
            }
        }

        IEnumerator ChargeAttack(float chargeDuration)
        {
            //플레이어 위치 기억
            Vector3 targetPos = playerPos.position;
            //돌진 효과 생성
            chargeEffect.enabled = true;
            //transform.position ~ playerpos.position 까지 이어지는 형태로 만들기
            chargeEffect.SetPosition(0, transform.position);
            chargeEffect.SetPosition(1, targetPos);
            //돌진 효과가 플레이어를 향하도록 설정
            chargeEffect.transform.LookAt(targetPos);
            //돌진 준비 시간 대기
            yield return new WaitForSeconds(chargePrepare);
            //효과 끄기 + 돌진 실행
            chargeEffect.enabled = false;
            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * copiedData.speed);
                yield return null;
            }
        }

        IEnumerator FireBullets(int bulletCount)
        {
            //보스 주위로 탄환 생성 후 발사
            //예 : 4면 사각형의 꼭짓점에 맞게 탄환 생성
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * 360f / bulletCount;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                // 탄환 생성
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.transform.SetParent(transform);
                //bullet.GetComponent<Bullet>().Initialize(direction);
            }
            yield return new WaitForSeconds(1f);
            //탄환 발사
            foreach(var bullet in transform)
            {
                //bullet.GetComponent<Bullet>().Fire();
            }
        }
        #endregion
    }
}
