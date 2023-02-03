using System;
using I2.Loc;
using VioletUI;

public class VersionView : StateMonobehaviour<UIState>.View
{
    private KothText Text
    {
        get
        {
            return this.text = ((this.text != null) ? this.text : base.GetComponentInChildren<KothText>());
        }
    }

    public extern void orig_Render(UIState state);
    protected override void Render(UIState state)
    {
        string translation = LocalizationManager.GetTranslation(MagicString.Version, false, 0, true, false, null, null);
        this.Text.text = translation + " " + Release.LongVersion;
        this.Text.text = this.Text.text + " Mod The Hat version 0.0.1";
    }

    protected override bool IsDirty(UIState state, UIState lastState)
    {
        return state.settings.language.current != lastState.settings.language.current;
    }

    public VersionView()
    {
    }

    private KothText text;
}
