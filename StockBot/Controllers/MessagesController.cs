using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace StockBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                DateTime dt = DateTime.Now;
                var year = dt.Year;
                var month = dt.Month;
                var day = dt.Date;
                Data data = new Data();
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                //await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                
                Activity reply = activity.CreateReply();
                if (Array.IndexOf(data.stockname, activity.Text) != -1)
                {
                    int local = Array.IndexOf(data.stockname, activity.Text);
                    string number = data.stocknumber[local];
                    var ttt = await PostToFunction(number,activity);
                    reply.Text ="查詢公司"+activity.Text+"\r\nTime："+ttt.time+"\r\nOpen："+ttt.Open+"\r\nHigh："+ttt.High+"\r\nLow："+ttt.Low+"\r\nClose："+ttt.Close;
                }
                else if (Array.IndexOf(data.stocknumber, activity.Text) != -1)
                {
                    var ttt = await PostToFunction(activity.Text, activity);
                    reply.Text = "查詢公司" + data.stockname[Array.IndexOf(data.stocknumber, activity.Text)] + "\r\nTime：" + ttt.time + "\r\nOpen：" + ttt.Open + "\r\nHigh：" + ttt.High + "\r\nLow：" + ttt.Low + "\r\nClose：" + ttt.Close;
                }
                else
                {
                    reply.Text = "找不到"+activity.Text+"的資訊";
                }

                //if( activity.Text == "2330")
                //{
                //    
                //    string body = JsonConvert.SerializeObject(stocknum).ToString();
                //    reply.Text = activity.Text;
                //}
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        public async Task<ReturnStock> PostToFunction(string stocknumber,Activity activity)
        {
            var client = new HttpClient();
            var uri = "https://21071108stock.azurewebsites.net/api/HttpTriggerPython31?code=VAYvJl5URU8D24mQRtv0gMLyM4wqYJCNxwq3TlIT1w8t0yxcytO0ig==";
            ReturnNum returnnum = new ReturnNum();
            returnnum.stocknum = stocknumber;
            string body = JsonConvert.SerializeObject(returnnum).ToString();
            byte[] byteData = Encoding.UTF8.GetBytes(body);
            var content = new ByteArrayContent(byteData);
            Activity datareply = activity.CreateReply();
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(uri,content);

            string stockjson = await response.Content.ReadAsStringAsync();
            ReturnStock returnStock = JsonConvert.DeserializeObject<ReturnStock>(stockjson);


            return returnStock;

        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
    public class ReturnNum
    {
        public string stocknum { get; set; }
    }
}