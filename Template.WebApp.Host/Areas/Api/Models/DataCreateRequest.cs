namespace Template.WebApp.Host.Areas.Api.Models;

// MVCのモデル検証はrecordのプロパティ側属性を無視(例外)するため、コンストラクタ引数に検証属性を付ける
public sealed record DataCreateRequest(
    [Required][MaxLength(Length.Name)] string Name,
    [Range(0, 999_999_999)] int Value);
