using UnityEngine;

/// <summary>
/// Localized text used in splash screen. This asset with corresponding text should reside in Resources folder
/// </summary>
[CreateScriptableObject]
public class SplashScreenSettings : ScriptableObject
{
#if ZEALOT_SimplifiedChinese
    public string loadingText = "载入中";
    public string downloadText = "更新中";
    public string patchWarningText = "您目前不在WIFI环境中，是否确定下载 {size} MB?";
    public string patchWarningButtonText = "繼續下載";//"移动蜂窝流量下载";
    public string patchWarningTitle = "不在WIFI环境中";
    public string resumePatchText = "网络中断，是否重新尝试？{newline}错误: {error}";
    public string resumePatchButtonText = "确定";
    public string resumePatchTitle = "网络中断";
    public string checkUpdates = "版本验证中，请稍后...";
    public string downloadingAssets = "更新档下载中(建议使用WiFi下载)...";
    public string loadingAssets = "游戏载入中(不消耗流量)...";
    public string downloadMovie = "更新下载中（建议使用WiFi下载）...";
    public string incorrectBuildVersion = "現在遊戲已有更新的版本";
    public string incorrectBuildVersionTitle = "现在游戏已有更新的版本";
#else
    public string loadingText = "載入中";
    public string downloadText = "更新中";
    public string patchWarningText = "您目前不在WIFI環境中，是否確定下載 {size} MB?";
    public string patchWarningButtonText = "行動流量下載";
    public string patchWarningTitle = "不在WIFI環境中";
    public string resumePatchText = "網絡中斷，是否重新嘗試？{newline}錯誤: {error}";
    public string resumePatchButtonText = "確定";
    public string resumePatchTitle = "網絡中斷";
    public string checkUpdates = "版本驗證中，請稍後...";
    public string downloadingAssets = "更新檔下載中(建議使用WiFi下載)...";
    public string loadingAssets = "遊戲載入中(不消耗流量)...";
    public string downloadMovie = "更新下載中（建議使用WiFi下載）...";
    public string incorrectBuildVersion = "現在遊戲已有更新的版本";
    public string incorrectBuildVersionTitle = "现在游戏已有更新的版本";
#endif
}
