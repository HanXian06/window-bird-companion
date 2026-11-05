namespace Bird.Core;

public sealed class FullscreenVisibility
{
    public bool ManuallyHidden {get;private set;}
    public void SetManualHidden(bool hidden)=>ManuallyHidden=hidden;
    public bool ShouldShow(bool enabled,bool otherAppFullscreen)=>!ManuallyHidden && !(enabled && otherAppFullscreen);
}
