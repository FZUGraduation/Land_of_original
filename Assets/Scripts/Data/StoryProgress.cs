

/// <summary>
/// 这个类用来存储剧情进度的字符串
/// </summary>
public static class StoryProgress
{
    public static readonly string heroSelect = GetStoryName("HeroSelect");//是否有选择初始英雄


    private static int index = 0;
    public static string GetStoryName(string str = "")
    {
        return $"story_{index++}_{str}";
    }
}
