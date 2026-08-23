namespace Template.WebApp.Host.Application;

public static class Messages
{
    // Validation

    public const string Required = "入力してください";

    public const string MaxLength = "{1}文字以内で入力してください";

    public const string Range = "{1}~{2}の範囲で入力してください";

    public static string MakeInvalid(string item) => $"{item}の形式が不正です";

    // Account

    public const string LoginFailed = "ログインに失敗しました";

    // Data

    public const string DuplicateName = "同じ名前のデータが存在します";
}
