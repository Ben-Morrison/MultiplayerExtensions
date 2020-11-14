using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.VOIP
{
    public class VoipPacket : INetSerializable, IPoolablePacket
    {
        public string? playerId;
        public byte[]? data;
        public int index;

        protected static PacketPool<VoipPacket> pool
        {
            get
            {
                return ThreadStaticPacketPool<VoipPacket>.pool;
            }
        }

        public static VoipPacket Create(string playerId, int index, byte[] data)
        {
            VoipPacket packet = pool.Obtain();
            packet.playerId = playerId;
            packet.index = index;
            packet.data = data;
            return packet;
        }

        public void Deserialize(NetDataReader reader)
        {
            playerId = reader.GetString();
            index = reader.GetInt();
            data = reader.GetBytesWithLength();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(playerId);
            writer.Put(index);
            if (data != null)
                writer.PutBytesWithLength(data, 0, data.Length);
            else
                Plugin.Log?.Warn($"Trying to serialize a 'VoipPacket' with null data.");
        }

        public void Release()
        {
            data = null;
            pool.Release(this);
        }
    }
}
