namespace Contract.DTOs.KafkaEvents;

public class Event
{
    public string Username { get; set; }
    
    public dynamic Data { get; set; }

    public DateTime DateTime { get; set; }

}