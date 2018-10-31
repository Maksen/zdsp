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
 
    string imagePath = "";
    Color lockedColor;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString(lockedColorHex, out lockedColor);
    }

    void Start()
    {
        button.onClick.AddListener(OnPressedCell);
    }

    /// <summary>
    /// Updates the content.
    /// </summary>
    /// <param name="itemData"></param>
    public override void UpdateContent(HeroCellDto itemData)
    {
        message.text = itemData.Message;

        if (Context != null)
        {
            var isSelected = Context.SelectedIndex == DataIndex;
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
 
            image.color = itemData.Unlocked ? Color.white : lockedColor;
        }
    }

    /// <summary>
    /// Updates the position.
    /// </summary>
    /// <param name="position"></param>
    public override void UpdatePosition(float position)
    {
        currentPosition = position;
        //if (gameObject.activeInHierarchy)
        //{
            animator.Play(scrollTriggerHash, -1, position);
            animator.speed = 0;
        //}
    }

    void OnPressedCell()
    {
        if (Context != null)
        {
            Context.OnPressedCell(this);
        }
    }

    // GameObject が非アクティブになると Animator がリセットされてしまうため
    // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
    float currentPosition = 0;
    void OnEnable()
    {
        UpdatePosition(currentPosition);
    }
}

