Java\protoc --java_out=output EpochProtocol.proto
Java\protoc --java_out=output LobbyProtocol.proto
Java\protoc --java_out=output GameProtocol.proto
pause

move output\EpochProtocol\*.java ..\..
move output\LobbyProtocol\*.java ..\..
move output\GameProtocol\*.java ..\..
pause