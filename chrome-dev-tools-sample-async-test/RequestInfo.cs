using System;

namespace chrome_dev_tools_sample_async_test
{
    public class RequestInfo
    {
        public RequestInfo()
        {
        }

        public RequestInfo(DateTime time, string url, string id, string body = null)
        {
            Id = id;
            Time = time;
            Url = url;
            Body = body;
        }

        public string Id { get; set; }
        public DateTime Time { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
        public string DataType { get; set; }

        public override string ToString()
        {
            return Time.ToString("HH.mm.ss") + " " + Url;
        }
    }
}