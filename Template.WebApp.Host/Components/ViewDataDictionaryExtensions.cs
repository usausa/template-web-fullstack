namespace Template.WebApp.Host.Components;

using Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class ViewDataDictionaryExtensions
{
    public static string GetTitle(this ViewDataDictionary viewData) => (string)viewData["Title"]!;

    public static void SetTitle(this ViewDataDictionary viewData, string value) => viewData["Title"] = value;
}
