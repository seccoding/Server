REM PacketGenerator ����
START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml

REM GenPackets.cs ����
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../Server/Packet"

REM xPacketManager.cs ����
XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"