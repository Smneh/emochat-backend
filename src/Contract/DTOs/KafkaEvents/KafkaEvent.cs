namespace Contract.DTOs.KafkaEvents;

public class KafkaEvent<TType> where TType : Enum
{
    public Event Event { get; set; } = default!;

    public TType EventType { get; set; } = default!;
}