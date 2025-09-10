using System;
using System.Collections.Generic;

namespace MiniHex.Shared
{
    // Frame: [STX=0xAA][CMD(1)][LEN(1)][PAYLOAD(LEN)][CHK(1)][ETX=0x55]
    // CHK = XOR of CMD, LEN and all PAYLOAD bytes (8-bit)
    public static class MiniFrame
    {
        public const byte STX = 0xAA;
        public const byte ETX = 0x55;

        public static byte XorChecksum(ReadOnlySpan<byte> data)
        {
            byte x = 0x00;
            foreach (var b in data) x ^= b;
            return x;
        }

        public static byte[] Build(byte cmd, ReadOnlySpan<byte> payload)
        {
            if (payload.Length > 255) throw new ArgumentException("payload too long");
            byte len = (byte)payload.Length;
            var buf = new byte[1 + 1 + 1 + payload.Length + 1 + 1];
            int o = 0;
            buf[o++] = STX;
            buf[o++] = cmd;
            buf[o++] = len;
            payload.CopyTo(buf.AsSpan(o));
            o += payload.Length;
            buf[o++] = XorChecksum(buf.AsSpan(1, 1 + 1 + payload.Length)); // XOR of CMD, LEN, PAYLOAD
            buf[o++] = ETX;
            return buf;
        }

        public static bool TryParse(ReadOnlySpan<byte> data, out byte cmd, out byte[] payload)
        {
            cmd = 0; payload = Array.Empty<byte>();
            if (data.Length < 1+1+1+1+1) return false;
            if (data[0] != STX || data[^1] != ETX) return false;

            cmd = data[1];
            byte len = data[2];
            int total = 1 + 1 + 1 + len + 1 + 1;
            if (data.Length != total) return false;

            var pay = data.Slice(3, len);
            byte chk = data[3 + len];
            byte calc = XorChecksum(data.Slice(1, 1 + 1 + len));
            if (chk != calc) return false;
            payload = pay.ToArray();
            return true;
        }

        public static IEnumerable<byte[]> ExtractFrames(List<byte> acc)
        {
            while (true)
            {
                int s = acc.IndexOf(STX);
                if (s < 0) { acc.Clear(); yield break; }
                int e = acc.IndexOf(ETX, s + 1);
                if (e < 0) yield break;

                var slice = acc.GetRange(s, e - s + 1).ToArray();
                acc.RemoveRange(0, e + 1);
                yield return slice;
            }
        }
    }

    public static class Cmd
    {
        // Client -> Server requests
        public const byte HELLO = 0x01;        // payload: optional ASCII
        public const byte GET_STATUS = 0x10;   // payload: -
        public const byte GET_NUMBER = 0x11;   // payload: -
        public const byte SET_STATUS = 0x20;   // payload: [state(1)] 1=IDLE,2=ACTIVE

        // Server -> Client responses
        public const byte HELLO_ACK = 0x81;    // payload: ASCII "hello~" or echo
        public const byte STATUS = 0x90;       // payload: [state(1)]
        public const byte NUMBER = 0x91;       // payload: [u32_le]
        public const byte ACK = 0xA0;          // payload: [state(1)] (for SET_STATUS)
        public const byte NACK = 0xA1;         // payload: [error(1)]
    }
}