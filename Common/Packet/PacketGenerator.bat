REM PacketGenerator ����
START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml ../../DummyClient/Packet ../../Server/Packet

REM GenPackets.cs ����
REM XCOPY /Y *.cs "../../DummyClient/Packet"
REM XCOPY /Y *.cs "../../Server/Packet"

REM xPacketManager.cs ����
REM XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
REM XCOPY /Y ServerPacketManager.cs "../../Server/Packet"