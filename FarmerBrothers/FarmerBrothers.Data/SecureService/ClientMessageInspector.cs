using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace FarmerBrothers.Data.SecureService
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        private const string USERNAME = @"UserName";
        private const string PASSWORD = @"Password";

        public string LastRequestXml { get; private set; }
        public string LastResponseXml { get; private set; }
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            LastResponseXml = reply.ToString();
        }

        public object BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;

            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;

                httpRequestMessage.Headers[USERNAME] = ConfigurationManager.AppSettings[USERNAME];
                httpRequestMessage.Headers[PASSWORD] = ConfigurationManager.AppSettings[PASSWORD];
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();

                httpRequestMessage.Headers[USERNAME] = ConfigurationManager.AppSettings[USERNAME];
                httpRequestMessage.Headers[PASSWORD] = ConfigurationManager.AppSettings[PASSWORD];

                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }

            return request;
        }
    }
}