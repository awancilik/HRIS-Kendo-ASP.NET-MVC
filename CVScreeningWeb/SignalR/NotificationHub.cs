using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningService.DTO.Notification;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Notification;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace CVScreeningWeb.SignalR
{
    public class NotificationHub : Hub
    {
        #region override_method
        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;
            SignalRUserMapper.Add(name, Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;
            SignalRUserMapper.Remove(name, Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;
            if (!SignalRUserMapper.GetConnections(name).Contains(Context.ConnectionId))
            {
                SignalRUserMapper.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
        #endregion

    }
}