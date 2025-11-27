using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HPBar : MonoBehaviour
{
    [Header("텍스트 설정")]
    public string Title = "HP";
    public Color TitleColor = Color.white;
    public Font TitleFont;
    public int TitleFontSize = 14;

    [Header("바 설정")]
    public Color BarColor = Color.green;
    public Color BarBackColor = Color.gray;
    public Sprite BarBackSprite;
    [Range(1f, 100f)]
    public int AlertPercent = 20;
    public Color AlertColor = Color.red;

    [Header("HP 수치 설정")]
    public float MaxValue = 100f;

    private float currentValue = 100f;
    private Image barFillImage, barBackgroundImage;
    private Text titleText;

    public float CurrentValue
    {
        get => currentValue;
        set
        {
            currentValue = Mathf.Clamp(value, 0, MaxValue);
            UpdateBar();
        }
    }

    private void Awake()
    {
        barFillImage = transform.Find("Bar").GetComponent<Image>();
        barBackgroundImage = transform.Find("BarBackground").GetComponent<Image>();
        titleText = transform.Find("Text").GetComponent<Text>();
    }

    private void Start()
    {
        // 초기 설정
        titleText.text = Title;
        titleText.color = TitleColor;
        titleText.font = TitleFont;
        titleText.fontSize = TitleFontSize;

        barFillImage.color = BarColor;
        barBackgroundImage.color = BarBackColor;
        barBackgroundImage.sprite = BarBackSprite;

        UpdateBar();
    }

    void UpdateBar()
    {
        if (barFillImage == null || titleText == null) return;

        float percent = currentValue / MaxValue;
        barFillImage.fillAmount = percent;

        string cur = Mathf.FloorToInt(currentValue).ToString("000");
        string max = Mathf.FloorToInt(MaxValue).ToString("000");
        titleText.text = $"{Title} {cur} / {max}";

        if (percent * 100f <= AlertPercent)
            barFillImage.color = AlertColor;
        else
            barFillImage.color = BarColor;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            // 에디터에서 미리보기 반영
            UpdateBar();
        }
    }
#endif
}
