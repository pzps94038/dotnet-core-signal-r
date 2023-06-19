using dotnet_core_signal_r.Core;
using dotnet_core_signal_r.Hubs;
using dotnet_core_signal_r.Model.Chat;
using dotnet_core_signal_r.Model.Shared;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_core_signal_r.Controllers;

[ApiController]
[Route("[controller]")]
public class Chat : ControllerBase
{

    [HttpGet]
    public BaseResponse<List<GetLiveChatRoomResponse>> GetLiveChatRoom()
    {
        var result = new BaseResponse<List<GetLiveChatRoomResponse>>();
        try
        {
            result.ReturnCode = ReturnCode.Success.Description();
            result.ReturnMessage = ReturnMessage.Success.Description();
            result.Data = ChatHub.liveChatRoom.Select(a=> new GetLiveChatRoomResponse 
            {
                ConnectionId = a.Key,
                Name = a.Value,
            }).ToList();
        }
        catch (Exception) 
        {
            result.ReturnCode = ReturnCode.Fail.Description();
            result.ReturnMessage = ReturnMessage.Fail.Description();
        }
        return result;
    }
}

