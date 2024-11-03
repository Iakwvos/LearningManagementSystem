namespace LMS.Web.Services;

public enum NotificationType
{
    Success,
    Error,
    Info,
    Warning
}

public class Notification
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public string Id { get; set; }

    public Notification(string message, NotificationType type)
    {
        Message = message;
        Type = type;
        Timestamp = DateTime.Now;
        Id = Guid.NewGuid().ToString();
    }
}

public class NotificationService
{
    public event Action<Notification>? OnNotification;

    public void Show(string message, NotificationType type = NotificationType.Info)
    {
        var notification = new Notification(message, type);
        OnNotification?.Invoke(notification);
    }

    public void ShowSuccess(string message) => Show(message, NotificationType.Success);
    public void ShowError(string message) => Show(message, NotificationType.Error);
    public void ShowWarning(string message) => Show(message, NotificationType.Warning);
} 