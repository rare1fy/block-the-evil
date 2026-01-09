using System;
using System.Collections.Generic;


//协议ID枚举
enum MESSAGE_ID
{
	//客户端登陆消息
	GAME_CMD_LOGIN_BYSESSION = 1,
	//同步玩家任务task
	GAMER_NOTIFY_TASK_INFO = 10101,
}

//协议类型，解析协议用
public static class MessageDef
{
	public static Dictionary<int, Type> MessageMap = new Dictionary<int, Type>()
	{
		//客户端登陆消息
		[(int)MESSAGE_ID.GAME_CMD_LOGIN_BYSESSION] = Type.GetType("GamerLoginS2C"),
		//同步玩家任务task
		[(int)MESSAGE_ID.GAMER_NOTIFY_TASK_INFO] = Type.GetType("NtfGamerTaskInfo"),
	};
}