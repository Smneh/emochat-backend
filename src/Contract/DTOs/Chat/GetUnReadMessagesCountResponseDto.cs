namespace Contract.DTOs.Chat;

public class GetUnReadMessagesCountResponseDto
{
    public long ChatCount { get; set; }
    public long GroupCount { get; set; }
    public long ProjectGroupCount { get; set; }
    public long SpecialGroupCount { get; set; }
    
}