
public class FrameEvent : SingletonEventCenter<FrameEvent>
{
    public static readonly string BeforeSceneLoder = GetEventName("BeforeSceneLoder");
    public static readonly string AfterSceneLoder = GetEventName("AfterSceneLoder");
}
