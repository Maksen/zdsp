using FancyScrollView;
using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class InterestScrollViewCell : FancyScrollViewCell<InterestCellDto, InterestScrollViewContext>
{
    [SerializeField]
    Animator animator;
    [SerializeField]
    Text message;
    [SerializeField]
    Image image;
    [SerializeField]
    Button button;
    [SerializeField]
    GameObject highlight;

    static readonly int scrollTriggerHash = Animator.StringToHash("scroll");
    InterestScrollViewContext context;

    void Start()
    {
        var rectTransform = transform as RectTransform;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchoredPosition3D = Vector3.zero;
        UpdatePosition(0);

        button.onClick.AddListener(OnPressedCell);
    }

    /// <summary>
    /// コンテキストを設定します
    /// </summary>
    /// <param name="context"></param>
    public override void SetContext(InterestScrollViewContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// セルの内容を更新します
    /// </summary>
    /// <param name="itemData"></param>
    public override void UpdateContent(InterestCellDto itemData)
    {
        message.text = itemData.Message;

        if (context != null)
        {
            var isSelected = context.SelectedIndex == DataIndex;
            highlight.SetActive(isSelected);
            HeroInterestType type = (HeroInterestType)itemData.Type;
            if (type == HeroInterestType.None)
            {
                ClientUtils.LoadIconAsync("UI_ZDSP_AsyncIcons/InterestCell/zzz_interestcell_test_Random.png", OnImageLoaded);
            }
            else
            {
                HeroInterestJson json = HeroRepo.GetInterestByType(type);
                if (json != null)
                    ClientUtils.LoadIconAsync(json.iconpath, OnImageLoaded);
            }
        }
    }

    private void OnImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            image.sprite = sprite;
    }

    /// <summary>
    /// セルの位置を更新します
    /// </summary>
    /// <param name="position"></param>
    public override void UpdatePosition(float position)
    {
        currentPosition = position;
        animator.Play(scrollTriggerHash, -1, position);
        animator.speed = 0;
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

