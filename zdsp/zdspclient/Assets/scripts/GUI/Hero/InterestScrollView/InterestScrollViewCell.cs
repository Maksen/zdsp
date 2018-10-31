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
    [SerializeField]
    Material grayScaleMat;
    [SerializeField]
    Sprite[] sprites;

    static readonly int scrollTriggerHash = Animator.StringToHash("scroll");

    void Start()
    {
        button.onClick.AddListener(OnPressedCell);
    }

    /// <summary>
    /// Updates the content.
    /// </summary>
    /// <param name="itemData"></param>
    public override void UpdateContent(InterestCellDto itemData)
    {
        message.text = itemData.Message;

        if (Context != null)
        {
            if (Context.HighlightSelected)
            {
                var isSelected = Context.SelectedIndex == DataIndex;
                highlight.SetActive(isSelected);
            }
            else
            {
                //highlight.SetActive(DataIndex == 0);
                highlight.SetActive(false);
            }

            HeroInterestJson json = HeroRepo.GetInterestByType((HeroInterestType)itemData.Type);
            if (json != null)
            {
                image.sprite = sprites[(int)json.interesttype];
                image.material = itemData.Applicable ? null : grayScaleMat;
            }
            else
            {
                image.sprite = null;
                image.material = grayScaleMat;
            }
        }
    }

    /// <summary>
    /// Updates the position.
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
        if (Context != null)
        {
            Context.OnPressedCell(this);
        }
    }
}

