namespace ServiceApis.Models
{
    public class ResultResponse<T>
    {        
            public T Data { get; set; }

            public string Message { get; set; }

            public bool IsSuccess { get; set; }
        public int responseCode { get; set; }

            //public int ReasonCode { get; set; }
    }

    public class ERFResponseClass
    {
        public int ERFId { get; set; }
        public int WorkorderId { get; set; }
    }

    public class IndexCounterModel
    {
        public int Indexid { get; set; }

        public string? IndexName { get; set; }

        public int? IndexValue { get; set; }

    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ResponseCode { get; set; }
    }
}
