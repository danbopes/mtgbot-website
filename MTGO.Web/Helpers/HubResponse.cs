namespace MTGO.Web.Helpers
{
    public class HubResponse
    {
        public bool Success = true;
        public string Message = null;
        public object Data { get; set; }

        public static HubResponse SuccessResponse(object data = null)
        {
            return new HubResponse
                {
                    Success = true,
                    Message = null,
                    Data = data
                };
        }

        public static HubResponse ErrorResponse(string message)
        {
            return new HubResponse
                {
                    Success = false,
                    Message = message
                };
        }
    }
}