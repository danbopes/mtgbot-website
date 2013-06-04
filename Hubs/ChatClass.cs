using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(string message)
        {
            // Call the addNewMessageToPage method to update clients.
            var context = Context.Request.GetHttpContext();

            if (context.Session["user_name"] != null )
                Clients.All.addNewMessageToPage(context.Session["user_name"], message);
            else
                Clients.All.addNewMessageToPage("Unknown", message);

        }
    }
}