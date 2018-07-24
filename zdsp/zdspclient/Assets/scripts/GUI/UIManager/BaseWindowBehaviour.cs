using UnityEngine;

/// <summary>
/// All UI window behaviour should extend from this
/// </summary>
public abstract class BaseWindowBehaviour : MonoBehaviour
{
    public bool hasBeenActive { get; set; }

    /// <summary>
    /// Called when window/dialog is registered to UIManager when UIHierarchy awake
    /// </summary>
    public virtual void OnRegister() { }

    /// <summary>
    /// Called when window/dialog is opened (window gameObject become active)
    /// </summary>
    public virtual void OnOpenWindow() { }

    /// <summary>
    /// Called when window/dialog is closed (window gameObject become inactive)
    /// </summary>
    public virtual void OnCloseWindow() { }

    /// <summary> 
    /// Called when window canvas is shown (window is opened or become top when previous top window closes) 
    /// Works for windows only.
    /// </summary>
    public virtual void OnShowWindow() { }

    /// <summary>
    /// Called when window canvas is hidden (window is closed or another window is opened on top)
    /// Works for windows only.
    /// </summary>
    public virtual void OnHideWindow() { }

    /// <summary>
    /// Called when scene change or quit game.
    /// Will only be called if window/dialog has previously been active
    /// </summary>
    public virtual void OnLevelChanged() { }
}
