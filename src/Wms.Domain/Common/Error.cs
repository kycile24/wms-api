namespace Wms.Domain.Common;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // Додаємо неявне перетворення зі string
    public static implicit operator Error(string message) => new("General.Error", message);
}