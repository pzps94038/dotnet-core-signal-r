using System;
using dotnet_core_signal_r.Core;
using dotnet_core_signal_r.Model.Shared;
using Microsoft.AspNetCore.SignalR;

namespace dotnet_core_signal_r.Hubs
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// 在線聊天室
        /// </summary>
        private static Dictionary<string, string> liveChatRoom = new Dictionary<string, string>();

        /// <summary>
        /// 加入聊天室
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<BaseResponse> JoinLiveChatRoom(string userName)
        {
            var result = new BaseResponse<string>();
            try
            {
                string? connectionId;
                if (liveChatRoom.TryGetValue(userName, out connectionId))
                {
                    result.ReturnCode = ReturnCode.Unique.Description();
                    result.ReturnMessage = ReturnMessage.Unique.Description();
                }
                else
                {
                    liveChatRoom.Add(userName, Context.ConnectionId);
                    // 更新在線清單給全部人
                    await Clients.All.SendAsync("PublicLiveChatRoom", liveChatRoom);
                    result.ReturnCode = ReturnCode.Success.Description();
                    result.ReturnMessage = ReturnMessage.CreateSuccess.Description();
                    result.Data = Context.ConnectionId;
                }
            }
            catch (Exception)
            {
                result.ReturnCode = ReturnCode.Fail.Description();
                result.ReturnMessage = ReturnMessage.Fail.Description();
            }

            return result;
        }

        /// <summary>
        /// 公開聊天室留言
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<BaseResponse> PublicChatSendMessage(string message)
        {
            var result = new BaseResponse();
            try
            {
                var userName = liveChatRoom.Where(a => a.Value == Context.ConnectionId).Select(a => a.Key).SingleOrDefault();
                if (userName != null)
                {
                    result.ReturnCode = ReturnCode.Success.Description();
                    result.ReturnMessage = ReturnMessage.Success.Description();
                    // 發送公開訊息給全部人
                    await Clients.All.SendAsync("PublicMessage", userName, message, DateTime.Now.ToString("HH:mm"));
                }
                else
                {
                    result.ReturnCode = ReturnCode.NotFound.Description();
                    result.ReturnMessage = ReturnMessage.NotFound.Description();
                }
            }
            catch (Exception)
            {
                result.ReturnCode = ReturnCode.Fail.Description();
                result.ReturnMessage = ReturnMessage.Fail.Description();
            }
            return result;
        }

        /// <summary>
        /// 私密聊天室留言
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<BaseResponse> PrivateChatSendMessage(string connectionId, string message)
        {
            var result = new BaseResponse();
            try
            {
                var privateUser = liveChatRoom.Where(a => a.Value == connectionId).Select(a => new
                {
                    Id = a.Value,
                    Name = a.Key
                }).SingleOrDefault();
                if (privateUser != null)
                {
                    var sendUser = liveChatRoom.Where(a => a.Value == Context.ConnectionId).Select(a => new
                    {
                        Id = a.Value,
                        Name = a.Key
                    }).Single();
                    result.ReturnCode = ReturnCode.Success.Description();
                    result.ReturnMessage = ReturnMessage.Success.Description();
                    // 發送私訊給指定人
                    await Clients.Client(privateUser.Id).SendAsync("PrivateMessage", sendUser.Name, message, DateTime.Now.ToString("HH:mm"));
                }
                else
                {
                    result.ReturnCode = ReturnCode.NotFound.Description();
                    result.ReturnMessage = ReturnMessage.NotFound.Description();
                }
            }
            catch (Exception)
            {
                result.ReturnCode = ReturnCode.Fail.Description();
                result.ReturnMessage = ReturnMessage.Fail.Description();
            }
            return result;
        }

        /// <summary>
        /// 離線處理
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = liveChatRoom.Where(a => a.Value == Context.ConnectionId).Select(a => a.Key).SingleOrDefault();
            if (userName != null)
            {
                liveChatRoom.Remove(userName);
                // 更新在線清單給全部人
                await Clients.All.SendAsync("PublicLiveChatRoom", liveChatRoom);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}

