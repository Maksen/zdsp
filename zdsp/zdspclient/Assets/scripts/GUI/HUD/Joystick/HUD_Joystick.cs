public class HUD_Joystick : BaseWidgetBehaviour
{
    private ZDSPJoystick joystick;

    private void Awake()
    {
        joystick = GetComponent<ZDSPJoystick>();
    }

    public override void OnLevelChanged()
    {
        joystick.ResetState();
    }
}