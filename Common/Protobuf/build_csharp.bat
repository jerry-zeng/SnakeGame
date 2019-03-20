CSharp\protogen -i:EpochProtocol.proto -o:output\EpochProtocol.cs
CSharp\protogen -i:LobbyProtocol.proto -o:output\LobbyProtocol.cs
CSharp\protogen -i:GameProtocol.proto -o:output\GameProtocol.cs
pause

move output\*.cs ..\..\Client\Snake\Assets\Scripts\Logic\Network\Protocol
pause
