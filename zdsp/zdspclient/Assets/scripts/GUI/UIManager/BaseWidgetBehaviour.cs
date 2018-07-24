using UnityEngine;

/// <summary>
/// All HUD widget behaviour should extend from this
/// </summary>
public abstract class BaseWidgetBehaviour : MonoBehaviour
{
    public bool hasBeenActive { get; set; }

    /// <summary>
    /// Called when scene change or quit game.
    /// Will only be called if widget has previously been active
    /// </summary>
    public virtual void OnLevelChanged() { }
}
