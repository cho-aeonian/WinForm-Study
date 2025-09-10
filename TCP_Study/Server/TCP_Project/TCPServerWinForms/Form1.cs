using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPServerWinForms
{
    public partial class Form1 : Form
    {
        private TcpListener listener;
        private bool isRunning = false;

        //여러개의 클라이언트 관리용
        private List<TcpClient> clients = new List<TcpClient>();
        private List<NetworkStream> streams = new List<NetworkStream>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void StopServer()
        {
            isRunning = false;

            foreach (var s in streams) { try { s.Close(); } catch { } }
            foreach (var c in clients) { try { c.Close(); } catch { } }

            streams.Clear();
            clients.Clear();

            if (listener != null)
            {
                try { listener.Stop(); } catch { }
                listener = null;
            }

            richTextBox1.AppendText("서버 종료~~~\n");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out int port))
            {
                MessageBox.Show("올바른 포트를 입력하세용");
                return;
            }

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            isRunning = true;
            richTextBox1.AppendText($"서버 시작 유후: {port}\n");

            _ = AcceptClientsAsync(); // 여러 클라이언트 수락
        }

        private async Task AcceptClientsAsync()
        {
            while (isRunning)
            {
                try
                {
                    var newClient = await listener.AcceptTcpClientAsync();
                    if (!isRunning) return;

                    var newStream = newClient.GetStream();

                    clients.Add(newClient);
                    streams.Add(newStream);

                    richTextBox1.Invoke((Action)(() =>
                    {
                        richTextBox1.AppendText("새 클라이언트 접속 !!!\n");
                    }));

                    _ = ReceiveMessagesAsync(newClient, newStream);
                }
                catch (ObjectDisposedException)
                {
                    break; // 서버가 종료된 경우
                }
                catch (Exception ex)
                {
                    richTextBox1.AppendText($"AcceptClientsAsync 오류: {ex.Message}\n");
                }
            }
        }

        private async Task ReceiveMessagesAsync(TcpClient client, NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (isRunning && client.Connected)
                {
                    int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    richTextBox1.Invoke((Action)(() =>
                    {
                        richTextBox1.AppendText($"클라이언트: {message}\n");
                    }));

                    // 다른 클라이언트에게 메시지 전달 (브로드캐스트)
                    foreach (var s in streams.ToList())
                    {
                        if (s != stream) // 자기 자신 제외
                        {
                            try
                            {
                                byte[] data = Encoding.UTF8.GetBytes(message);
                                await s.WriteAsync(data, 0, data.Length);
                            }
                            catch
                            {
                                // 전송 실패하면 무시
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                // 클라이언트 종료 시 리스트에서 제거
                streams.Remove(stream);
                clients.Remove(client);
                try { stream.Close(); client.Close(); } catch { }

                richTextBox1.Invoke((Action)(() =>
                {
                    richTextBox1.AppendText("클라이언트 연결 종료\n");
                }));
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string message = textBox2.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            byte[] buffer = Encoding.UTF8.GetBytes(message);

            foreach (var s in streams.ToList())
            {
                try
                {
                    await s.WriteAsync(buffer, 0, buffer.Length);
                }
                catch { }
            }

            richTextBox1.AppendText($"서버: {message}\n");
            textBox2.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StopServer();
            Application.Exit();
        }
    }
}
