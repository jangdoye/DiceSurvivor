using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    [Header("Title Setting")]
    public string Title = "HP";
    public Color TitleColor = Color.white;
    public Font TitleFont;
    public int TitleFontSize = 14;

    [Header("Bar Setting")]
    public Color BarColor = Color.green;
    public Color BarBackGroundColor = Color.gray;
    public Sprite BarBackGroundSprite;
    [Range(1f, 100f)]
    public int Alert = 20;  // 퍼센트 기준
    public Color BarAlertColor = Color.red;

    [Header("Sound Alert")]
    public AudioClip sound;
    public bool repeat = false;
    public float RepeatRate = 1f;

    [Header("HP 값 설정")]
    public float MaxValue = 100f;

    private Image bar, barBackground;
    private float nextPlay;
    private Text txtTitle;
    private float barValue;

    public float BarValue
    {
        get { return barValue; }
        set
        {
            barValue = Mathf.Clamp(value, 0, MaxValue);
            UpdateValue(barValue);
        }
    }

    private void Awake()
    {
        bar = transform.Find("Bar").GetComponent<Image>();
        barBackground = transform.Find("BarBackground").GetComponent<Image>();
        txtTitle = transform.Find("Text").GetComponent<Text>();
        // audiosource = GetComponent<AudioSource>();  // 필요시 사용
    }

    private void Start()
    {
        txtTitle.text = Title;
        txtTitle.color = TitleColor;
        txtTitle.font = TitleFont;
        txtTitle.fontSize = TitleFontSize;

        bar.color = BarColor;
        barBackground.color = BarBackGroundColor;
        barBackground.sprite = BarBackGroundSprite;

        UpdateValue(barValue);
    }

    void UpdateValue(float val)
    {
        // 게이지 반영
        bar.fillAmount = val / MaxValue;

        // 텍스트 형식: "HP 075 / 100"
        string current = Mathf.FloorToInt(val).ToString("000");
        string max = Mathf.FloorToInt(MaxValue).ToString("000");
        txtTitle.text = $"{Title} {current} / {max}";

        // 경고 색상 처리
        float percent = (val / MaxValue) * 100f;
        if (Alert >= percent)
        {
            bar.color = BarAlertColor;
        }
        else
        {
            bar.color = BarColor;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateValue(MaxValue / 2f); // 에디터에서 미리보기
            txtTitle.color = TitleColor;
            txtTitle.font = TitleFont;
            txtTitle.fontSize = TitleFontSize;

            bar.color = BarColor;
            barBackground.color = BarBackGroundColor;
            barBackground.sprite = BarBackGroundSprite;
        }
        else
        {
            if (Alert >= (barValue / MaxValue) * 100f && Time.time > nextPlay)
            {
                nextPlay = Time.time + RepeatRate;
                // audiosource.PlayOneShot(sound);  // 필요 시 활성화
            }
        }
    }
}
