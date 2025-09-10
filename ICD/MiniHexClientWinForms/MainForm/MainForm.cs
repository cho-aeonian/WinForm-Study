using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniHex.Shared;
using System.Buffers.Binary;

namespace MiniHexClientWinForms
{
    public partial class MainForm : Form
    {
        TcpClient? _client;
        NetworkStream? _ns;
        volatile bool _running = false;

        public MainForm()
        {
            InitializeComponent();
            grpQuick.Hide();
            grpSend.Hide();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _client?.Dispose();
                _client = new TcpClient();
                await _client.ConnectAsync(txtHost.Text, int.Parse(txtPort.Text));
                _ns = _client.GetStream();
                _running = true;
                Log($"[Client] connected to {txtHost.Text}:{txtPort.Text}");
                _ = Task.Run(RecvLoop);
                ToggleUi(true);
            }
            catch (Exception ex)
            {
                Log("! connect failed: " + ex.Message);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _running = false;
            _ns?.Close();
            _client?.Close();
            ToggleUi(false);
            Log("[Client] disconnected");
        }

        private void ToggleUi(bool connected)
        {
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            grpSend.Enabled = connected;
            grpQuick.Enabled = connected;
        }

        private async Task RecvLoop()
        {
            var buf = new byte[2048];
            var acc = new System.Collections.Generic.List<byte>();
            try
            {
                while (_running && _ns != null)
                {
                    int n = await _ns.ReadAsync(buf, 0, buf.Length);
                    if (n == 0) break;
                    acc.AddRange(buf.AsSpan(0, n).ToArray());
                    foreach (var raw in MiniHex.Shared.MiniFrame.ExtractFrames(acc))
                    {
                        if (MiniFrame.TryParse(raw, out var cmd, out var payload))
                        {
                            if (cmd == Cmd.STATUS && payload.Length >= 1)
                            {
                                byte st = payload[0];
                                string name = st == 1 ? "IDLE" : (st == 2 ? "ACTIVE" : $"UNK({st})");
                                Log($"[Rx] STATUS: {st} ({name})");
                            }
                            else if (cmd == Cmd.NUMBER && payload.Length >= 4)
                            {
                                uint v = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(0,4));
                                Log($"[Rx] NUMBER: {v}");
                            }
                            else if (cmd == Cmd.HELLO_ACK)
                            {
                                string s = Encoding.ASCII.GetString(payload);
                                Log($"[Rx] HELLO_ACK: \"{s}\"");
                            }
                            else if (cmd == Cmd.ACK && payload.Length >= 1)
                            {
                                Log($"[Rx] ACK: state={payload[0]}");
                            }
                            else if (cmd == Cmd.NACK && payload.Length >= 1)
                            {
                                Log($"[Rx] NACK: err=0x{payload[0]:X2}");
                            }
                            else
                            {
                                Log($"[Rx] {BitConverter.ToString(raw)}");
                            }
                        }
                        else
                        {
                            Log($"[Rx] invalid: {BitConverter.ToString(raw)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("! recv error: " + ex.Message);
            }
        }

        private void btnBuildSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (_ns == null) return;
                byte cmd = ParseByte(txtCmdHex.Text);
                byte[] payload = ParseBytes(txtPayloadHex.Text);
                var frame = MiniFrame.Build(cmd, payload);
                _ns.Write(frame, 0, frame.Length);
                Log("[Tx] " + BitConverter.ToString(frame));
            }
            catch (Exception ex)
            {
                Log("! build/send error: " + ex.Message);
            }
        }

        private void btnSendRaw_Click(object sender, EventArgs e)
        {
            try
            {
                if (_ns == null) return;
                byte[] raw = ParseBytes(txtRawHex.Text);
                _ns.Write(raw, 0, raw.Length);
                Log("[TxRaw] " + BitConverter.ToString(raw));
            }
            catch (Exception ex)
            {
                Log("! send raw error: " + ex.Message);
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }


        private void btnHello_Click(object sender, EventArgs e) => Send(Cmd.HELLO, Encoding.ASCII.GetBytes("hello?"));
        private void btnGetStatus_Click(object sender, EventArgs e) => Send(Cmd.GET_STATUS, Array.Empty<byte>());
        private void btnGetNumber_Click(object sender, EventArgs e) => Send(Cmd.GET_NUMBER, Array.Empty<byte>());
        private void btnSetStatus1_Click(object sender, EventArgs e) => Send(Cmd.SET_STATUS, new byte[]{ 1 });
        private void btnSetStatus2_Click(object sender, EventArgs e) => Send(Cmd.SET_STATUS, new byte[]{ 2 });

        private void Send(byte cmd, ReadOnlySpan<byte> payload)
        {
            if (_ns == null) return;
            var frame = MiniFrame.Build(cmd, payload);
            _ns.Write(frame, 0, frame.Length);
            Log("[Tx] " + BitConverter.ToString(frame));
        }

        private void Log(string s)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(Log), s); return; }
            txtLog.AppendText(s + Environment.NewLine);
        }

        private static byte ParseByte(string token)
        {
            token = (token ?? "").Trim();
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) token = token[2..];
            if (token.Length == 1) token = "0" + token;
            if (!byte.TryParse(token, System.Globalization.NumberStyles.HexNumber, null, out var v))
                throw new Exception("invalid hex byte: " + token);
            return v;
        }

        private static byte[] ParseBytes(string text)
        {
            text = (text ?? "");
            var hex = new StringBuilder();
            foreach (char ch in text)
                if (Uri.IsHexDigit(ch)) hex.Append(ch);
            if (hex.Length == 0) return Array.Empty<byte>();
            if (hex.Length % 2 != 0) throw new Exception("hex length must be even");
            var bytes = new byte[hex.Length/2];
            for (int i=0; i<bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.ToString(i*2,2), 16);
            return bytes;
        }
    }
}