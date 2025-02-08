namespace expense_tracker.Dtos.Graph;

public class CategoryWiseSummaryResponseDto: ResponseDto
{
    public List<CategorySummaryDto> Data { get; set; } = new();
};

