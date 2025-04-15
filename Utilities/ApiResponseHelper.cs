namespace expense_tracker.Utilities;

public abstract class ApiResponseHelper
{
    public static ResponseDto GenerateUserNotFoundResponse(int id = -1)
    {
        if (id == -1)
            return new ResponseDto()
            {
                Success = false,
                StatusCode = 404,
                Message = "User not found",
                Errors = ["User not found"],
            };

        return new ResponseDto()
        {
            Success = true,
            StatusCode = 404,
            Message = "User not found",
            Errors = [$"User with id #{id} not found"],
        };
    }
    public static ResponseDto GenerateTransactionDeletedSuccessResponse(int transactionId)
    {
        return new ResponseDto()
        {
            Success = true,
            StatusCode = 200,
            Message = $"Transaction #{transactionId} has been deleted successfully",
            Errors = []
        };
    }
    public static ResponseDto GenerateTransactionNotFoundResponse()
    {
        return new ResponseDto()
        {
            Success = false,
            StatusCode = 404,
            Message = "Transaction not found",
            Errors = ["Transaction not found"],
        };
    }
    public static ResponseDto GenerateTransactionPurposeOrTypeErrorResponse()
    {
        return new ResponseDto()
        {
            Success = false,
            StatusCode = 400,
            Message = "Invalid transaction purpose or type",
            Errors = ["Invalid transaction purpose or type"],
        };
    }

    public static ResponseDto GenerateTransactionMarkedSuccessResponse()
    {
        return new ResponseDto()
        {
            Success = true,
            StatusCode = 200,
            Message = "Transaction has marked successfully",
            Errors = []
        };
    }
}