namespace TaskManagement.Functions.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ApiResponse<T> SuccessResult(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public new static ApiResponse Success(string message = "Operação realizada com sucesso")
        {
            var response = new ApiResponse();
            response.SetSuccess(true);
            response.Message = message;
            return response;
        }

        public static ApiResponse Failure(string message, List<string>? errors = null)
        {
            var response = new ApiResponse();
            response.SetSuccess(false);
            response.Message = message;
            response.Errors = errors ?? new List<string>();
            return response;
        }

        private void SetSuccess(bool success)
        {
            base.Success = success;
        }
    }
}
