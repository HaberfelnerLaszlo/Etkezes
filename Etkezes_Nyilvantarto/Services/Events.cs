namespace Etkezes_Nyilvantarto.Services
{
    public class OnMessageEventArgs : EventArgs
    {
        public string Message { get; }
        public int Code { get; }
        public OnMessageEventArgs(string message, int code = 0)
        {
            Message = message;
            Code = code;
        }
    }
}
