using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;

    //메인 메뉴 씬을 관리하는 클래스
    public class MainMenu : MonoBehaviour
    {
        #region Variables
        //참조
        private AudioManager audioManager;

        //씬 변경
        public SceneFader fader;
        [SerializeField]
        private string loadToScene = "Lobby";

        //메뉴
        public GameObject mainMenuUI;
        public GameObject optionUI;
        public GameObject customizePanel;
        public GameObject previewCharacter;
        public GameObject thePlayer;
        public GameObject gameName;
    /*public GameObject loadGameButton;
    public GameObject creditCanvas;*/

        private bool isShowOption = false;
        private bool isShowCredit = false;

        //볼륨 조절
        public AudioMixer audioMixer;

       // public Slider bgmSlider;
        //public Slider sfxSlider;
       // public Slider masterSlider;

        //게임 데이터
        private int sceneNumber;
        #endregion

        #region Unity Event Method
        private void Start()
        {
        /*
            //게임데이터 가져와서 초기화 하기
            GameDataInit();

            //메뉴 UI 셋팅
            if (sceneNumber >= 0)
            {
                loadGameButton.SetActive(true);
            }
            else
            {
                loadGameButton.SetActive(false);
            }
        */
            //참조
            audioManager = AudioManager.Instance;

            //씬 시작시 페이드인 효과
            fader.FadeStart();

            //메뉴 배경음 플레이
            audioManager.PlayBgm("MainMenu");

            //초기화
         /*   isShowOption = false;
            isShowCredit = false;*/
        }

        private void Update()
        {
            //esc키를 누르면
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isShowOption)
                {
                    HideOptionUI();
                }
              /*  else if (isShowCredit)
                {
                    HideCreditUI();
                }*/
            }
        }
        #endregion

        #region Custom Method
        //게임데이터 가져와서 초기화 하기
        private void GameDataInit()
        {
            //옵션 저장값 가져와서 게임에 적용
            //LoadOptions();

            //게임 플레이 저장값 가져오기: 빌드번호
            //PlayerPrefs 모드
            //sceneNumber = PlayerPrefs.GetInt("SceneNumber", -1);
            //FileSystem 모드
            //PlayData playData = SaveLoad.LoadData();
            //PlayerDataManager.Instance.InitPlayerData(playData);
            //sceneNumber = PlayerDataManager.Instance.SceneNumber;
        }

        public void PlayGame()
        {
            //메뉴 선택 사운드
            audioManager.StopBgm();
            audioManager.Play("ButtonClick");

            //새게임 하러 가기
            fader.FadeTo(loadToScene);
        }

    public void ShowCustomizeUI()
    {
        mainMenuUI.SetActive(false);
        customizePanel.SetActive(true);
        previewCharacter.SetActive(true);
        thePlayer.SetActive(false);
        gameName.SetActive(false);

        audioManager.Play("ButtonClick");
    }
    public void HideCustomizeUI()
    {
        customizePanel.SetActive(false);
        mainMenuUI.SetActive(true);
        previewCharacter.SetActive(false);
        thePlayer.SetActive(true);
        gameName.SetActive(true);

        audioManager.Play("ButtonClick");
    }
    

        public void Options()
        {
            //메뉴 선택 사운드
            audioManager.Play("ButtonClick");

            //옵션 UI 보여주기
            isShowOption = true;
            mainMenuUI.SetActive(false);
            optionUI.SetActive(true);

        }
    /*  public void Credits()
      {
          //메뉴 선택 사운드
         // audioManager.Play("MenuSelect");

          //크레딧 UI 보여주기
          StartCoroutine(ShowCreditUI());
      }
  */
    public void QuitGame()
        {
            //TODO: Cheating
            PlayerPrefs.DeleteAll();

            //Debug.Log("Quit Game!!!");
            Application.Quit();
        }

        //옵션 UI 나가기
        public void HideOptionUI()
        {
          //  audioManager.Play("ButtonClick");
            optionUI.SetActive(false);
            mainMenuUI.SetActive(true);

            isShowOption = false;
        }

        //Bgm 볼륨 조절
        /*public void SetBgmVolume(float value)
        {
            //볼륨값 저장
            PlayerPrefs.SetFloat("Bgm", value);

            //볼륨 조절
            audioMixer.SetFloat("Bgm", value);
        }

        //Sfx 볼륨 조절
        public void SetSfxVolume(float value)
        {
            //볼륨값 저장
            PlayerPrefs.SetFloat("Sfx", value);

            //볼륨 조절
            audioMixer.SetFloat("Sfx", value);
        }
        //Master 볼륨 조절
        public void SetMasterVolume(float value)
        {
            //볼륨값 저장
            PlayerPrefs.SetFloat("Master", value);

            //볼륨 조절
            audioMixer.SetFloat("Master", value);
        }

        //옵션 저장값들을 가져와서 게임에 적용한다
        private void LoadOptions()
        {
            //배경음 볼륨값 가져오기
            float bgmVolume = PlayerPrefs.GetFloat("Bgm", 0f);
            //오디오 믹서에 적용
            SetBgmVolume(bgmVolume);
            //UI에 적용
            bgmSlider.value = bgmVolume;

            //효과음 볼륨값 가져오기
            float sfxVolume = PlayerPrefs.GetFloat("Sfx", 0f);
            //오디오 믹서에 적용
            SetSfxVolume(sfxVolume);
            //UI에 적용
            sfxSlider.value = sfxVolume;

            //효과음 볼륨값 가져오기
         //   float masterVolume = PlayerPrefs.GetFloat("AudioMaster", 0f);
            //오디오 믹서에 적용
           // SetMasterVolume(masterVolume);
            //UI에 적용
            //masterSlider.value = masterVolume;
            //기타...
        }

        //크레딧 UI 보여주기
        IEnumerator ShowCreditUI()
        {
            isShowCredit = true;

            mainMenuUI.SetActive(false);
            creditCanvas.SetActive(true);

            yield return new WaitForSeconds(10f);

            HideCreditUI();
        }

        //크레딧 UI 나가기
        public void HideCreditUI()
        {
            creditCanvas.SetActive(false);
            mainMenuUI.SetActive(true);

            isShowCredit = false;
        }*/
        #endregion
    }

