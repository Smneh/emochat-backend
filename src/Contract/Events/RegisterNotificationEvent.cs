namespace Contract.Events;

public class RegisterNotificationEvent
{
    public string Content { get; set; } 
    public string Sender { get; set; } 
    public List<string>  Receivers { get; set; } 
    public string MetaInfo { get; set; } 
    public string Type { get; set; } 
    public string Category { get; set; }
}