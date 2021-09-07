namespace Deepwell.Common.CommonModel
{
    public class ResponseResult
    {
        public ResponseResult()
        {
            Success = true;
        }

        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; } = true;
    }
}
