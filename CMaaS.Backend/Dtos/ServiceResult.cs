namespace CMaaS.Backend.Dtos
{
    /// <summary>
    /// Standardized service layer response wrapper
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
