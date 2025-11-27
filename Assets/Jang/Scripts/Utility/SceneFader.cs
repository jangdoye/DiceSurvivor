using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

    //씬 시작시 페이드인, 씬 종료시 페이드 아웃 효과 구현
    public class SceneFader : MonoBehaviour
    {
        #region Field
        //페이더 이미지
        public Image img;

        //애니메이션 커브
        public AnimationCurve curve;

        //페이드인 딜레이 타임
        // [SerializeField]
        // private float fadeInDelay = 5f;

        [SerializeField]
        private bool isWhite = false;
        #endregion

        private void Start()
        {
            //초기화 - 페이드 이미지
            img.color = new Color(0f, 0f, 0f, 1f);

            //페이드인
            //StartCoroutine(FadeIn(fadeInDelay));
            FadeStart(0.3f);
        }

        //코루틴으로 구현
        //delayTime : 매개변수로 딜레이 타임받아 딜레이 후 페이드 효과
        //FadeIn : 1초동안 : 검정에서 완전 투명으로 (이미지 알파값 a:1 -> a:0)
        IEnumerator FadeIn(float delayTime = 0f, bool white = false)
        {
            //delayTime 지연
            if(delayTime > 0)
            {
                yield return new WaitForSeconds(delayTime);
            }

            float t = 1f;

            while(t > 0)
            {
                t -= Time.deltaTime;
                float a = curve.Evaluate(t);
                if (!white)
                {
                    img.color = new Color(0f, 0f, 0f, a);
                }
                else
                {
                    img.color = new Color(1f, 1f, 1f, a);
                }

                yield return 0f;    //한프레임 지연
            }
        }

        //페이드인 외부에서 호출
        public void FadeStart(float delayTime = 0f, bool white = false)
        {
            StartCoroutine(FadeIn(delayTime, white));
        }

        //FadeOut 효과 후 매개변수로 받은 씬이름으로 LoadScene으로 이동
        IEnumerator FadeOut(string sceneName)
        {
            //FadeOut 효과 후
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime;
                float a = curve.Evaluate(t);
                if (!isWhite)
                {
                    img.color = new Color(0f, 0f, 0f, a);
                }
                else
                {
                    img.color = new Color(1f, 1f, 1f, a);
                }

                    yield return 0f;
            }

            //씬이동
            if(sceneName != "")
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        //FadeOut 효과 후 매개변수로 받은 씬 빌드번호로 LoadScene으로 이동
        IEnumerator FadeOut(int sceneNumber)
        {
            //FadeOut 효과 후
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime;
                float a = curve.Evaluate(t);
                if (!isWhite)
                {
                    img.color = new Color(0f, 0f, 0f, a);
                }
                else
                {
                    img.color = new Color(1f, 1f, 1f, a);
                }

                yield return 0f;
            }

            //씬이동
            if (sceneNumber >= 0)
            {
                SceneManager.LoadScene(sceneNumber);
            }
        }

        //다른 씬으로 이동시 호출 - 씬 이름
        public void FadeTo(string sceneName = "")
        {            
            StartCoroutine(FadeOut(sceneName));
        }

        //다른 씬으로 이동시 호출 - 씬 빌드 인덱스
        public void FadeTo(int sceneNumber = -1)
        {
            StartCoroutine(FadeOut(sceneNumber));
        }

    }