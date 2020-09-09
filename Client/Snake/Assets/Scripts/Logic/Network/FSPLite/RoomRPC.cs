namespace Framework.Network
{
    public static class RoomRPC
    {
        // S2C
        public const string RPC_UpdateRoomInfo = "_RPC_UpdateRoomInfo";
        public const string RPC_NotifyGameStart = "_RPC_NotifyGameStart";
        public const string RPC_NotifyGameResult = "_RPC_NotifyGameResult";

        public const string RPC_JoinRoom = "_RPC_JoinRoom";  //C2S
        public const string RPC_OnJoinRoom = "_RPC_OnJoinRoom";  //S2C

        public const string RPC_ExitRoom = "_RPC_ExitRoom";  //C2S
        public const string RPC_OnExitRoom = "_RPC_OnExitRoom";  //S2C

        public const string RPC_RoomReady = "_RPC_RoomReady";  //C2S
        public const string RPC_CancelReady = "_RPC_CancelReady";  //C2S

        public const string RPC_Ping = "_RPC_Ping";  //C2S
        public const string RPC_Pong = "_RPC_Pong";  //S2C
    }
}