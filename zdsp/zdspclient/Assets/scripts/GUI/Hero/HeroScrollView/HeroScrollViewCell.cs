using FancyScrollView;
using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class HeroScrollViewCell : FancyScrollViewCell<HeroCellDto, HeroScrollViewContext>
{
    [SerializeField] Animator animator;
    [SerializeField] Text message;
    [SerializeField] Image image;
    [SerializeField] Button button;
    [SerializeField] GameObject highlight;
    [SerializeField] string lockedColorHex;

    static readonly int scrollTriggerHash = Animator.StringToHash("scroll");
    HeroScrollViewContext context;
    string imagePath = "";
    Color lockedColor;

    void Start()
    {
        UpdatePosition(0);
        button.onClick.AddListener(OnPressedCell);
        ColorUtility.TryParseHtmlString(lockedColorHex, out lockedColor);
    }

    /// <summary>
    /// コンテキストを設定します
    /// </summary>
    /// <param name="context"></param>
    public override void SetContext(HeroScrollViewContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// セルの内容を更新します
    /// </summary>
    /// <param name="itemData"></param>
    public override void UpdateContent(HeroCellDto itemData)
    {
        message.text = itemData.Message;

        if (context != null)
        {
            var isSelected = context.SelectedIndex == DataIndex;
            highlight.SetActive(isSelected);

            HeroJson heroJson = HeroRepo.GetHeroById(itemData.HeroId);
            if (heroJson != null)
            {
                if (imagePath != heroJson.portraitpath)
                {
                    Sprite sprite = ClientUtils.LoadIcon(heroJson.portraitpath);
                    if (sprite != null)
                        image.sprite = sprite;
                }
            }
            else  // temp to be removed
            {
                image.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/HeroCell/zzz_herocell_test.png");
            }
            image.color = itemData.Unlocked ? Color.white : lockedColor;
        }
    }

    /// <summary>
    /// セルの位置を更新します
    /// </summary>
    /// <param name="position"></param>
    public override void UpdatePosition(float position)
    {
        currentPosition = position;
        if (gameObject.activeInHierarchy)
        {
            animator.Play(scrollTriggerHash, -1, position);
            animator.speed = 0;
        }
    }

    // GameObject が非アクティブになると Animator がリセットされてしまうため
    // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
    float currentPosition = 0;
    void OnEnable()
    {
        UpdatePosition(currentPosition);
    }

    void OnPressedCell()
    {
        if (context != null)
        {
            context.OnPressedCell(this);
        }
    }
}

