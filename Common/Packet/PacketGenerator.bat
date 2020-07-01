REM PacketGenerator 실행
START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml

REM GenPackets.cs 복사
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../Server/Packet"

REM xPacketManager.cs 복사
XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"