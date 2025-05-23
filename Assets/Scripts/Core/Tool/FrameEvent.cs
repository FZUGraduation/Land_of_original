
public class FrameEvent : SingletonEventCenter<FrameEvent>
{
    public static readonly string BeforeSceneLoder = GetEventName("BeforeSceneLoder");
    public static readonly string AfterSceneLoder = GetEventName("AfterSceneLoder");
    public static readonly string CreateWorldPlayer = GetEventName("CreateWorldPlayer");
    public static readonly string ResetHeroBody = GetEventName("ResetHeroBody");
    public static readonly string SlotSelect = GetEventName("SlotSelect");
    public static readonly string MoveEnable = GetEventName("MoveEnable");
}
