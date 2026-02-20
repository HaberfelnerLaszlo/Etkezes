namespace Etkezes_Models
{
    public class MainResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public void Clear()
        {
            this.Success = false;
            this.Message = string.Empty;
            this.Data = null;
        }
    }
}
