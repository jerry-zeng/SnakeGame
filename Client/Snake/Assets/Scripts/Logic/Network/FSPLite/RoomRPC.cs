namespace Framework.Network
{
    public static class RoomRPC
    {
        public const string RPC_UpdateRoomInfo = "_RPC_UpdateRoomInfo";
        public const string RPC_NotifyGameStart = "_RPC_NotifyGameStart";
        public const string RPC_NotifyGameResult = "_RPC_NotifyGameResult";

        public const string RPC_JoinRoom = "_RPC_JoinRoom";
        public const string RPC_OnJoinRoom = "_RPC_OnJoinRoom";

        public const string RPC_ExitRoom = "_RPC_ExitRoom";
        public const string RPC_OnExitRoom = "_RPC_OnExitRoom";

        public const string RPC_RoomReady = "_RPC_RoomReady";
        public const string RPC_CancelReady = "_RPC_CancelReady";

        public const string RPC_Ping = "_RPC_Ping";
        public const string RPC_OnPing = "_RPC_OnPing";
    }
}