Cpp\protoc --cpp_out=output EpochProtocol.proto
Cpp\protoc --cpp_out=output LobbyProtocol.proto
Cpp\protoc --cpp_out=output GameProtocol.proto
pause

::move output\*.pb.h ..\..\Client\Snaker\Assets\Scripts\Logic\Network\Protocol
::move output\*.pb.cc ..\..\Client\Snaker\Assets\Scripts\Logic\Network\Protocol
::pause
