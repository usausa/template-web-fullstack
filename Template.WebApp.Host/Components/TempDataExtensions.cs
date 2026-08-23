namespace Template.WebApp.Host.Components;

using Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class TempDataExtensions
{
    public static void SetMessage(this ITempDataDictionary tempData, string message)
    {
        tempData["Message"] = message;
    }

    public static string GetMessage(this ITempDataDictionary tempData) => (string)tempData["Message"]!;
}
