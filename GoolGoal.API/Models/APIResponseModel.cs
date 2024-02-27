namespace GoolGoal.API.Models
{
    public class APIResponseModel
    {
        public object StatusCode { get; set; }
        public string Message { get; set; }
        public object Result { get; set; } = "";
    }
}
