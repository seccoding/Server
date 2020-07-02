REM PacketGenerator 실행
START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml ../../DummyClient/Packet ../../Server/Packet

REM GenPackets.cs 복사
REM XCOPY /Y *.cs "../../DummyClient/Packet"
REM XCOPY /Y *.cs "../../Server/Packet"

REM xPacketManager.cs 복사
REM XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
REM XCOPY /Y ServerPacketManager.cs "../../Server/Packet"