using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPClientWinForms
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string ip = textBox1.Text.Trim();
                if (!int.TryParse(textBox2.Text, out int port))
                {
                    MessageBox.Show("올바른 포트 번호 입력해주세용");
                    return;
                }

                client = new TcpClient();
                await client.ConnectAsync(ip, port);
                stream = client.GetStream();

                richTextBox1.AppendText($"서버 연결 성공~~: {ip}:{port}\n");
                _ = ReceiveMessagesAsync();

            }
            catch (Exception ex)
            {
                MessageBox.Show("연결실패ㅋ: " + ex.Message);
            }
        }

        //서버 메시지 수신
        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (client != null && client.Connected)
                {
                    int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0)
                    {
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    richTextBox1.Invoke((Action)(() =>
                    {
                        richTextBox1.AppendText($"서버: {message}\n");
                    }));
                }
            }

            catch
            {
                //수신 중 예외 발생 시 무시!!!
            }
            finally
            {
                richTextBox1.Invoke((Action)(() =>
                {
                    richTextBox1.AppendText("서버 연결 종료~~\n");
                }));
                DisconnectClient();
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (client == null || !client.Connected)
            {
                return;
            }

            string message = textBox3.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);

            richTextBox1.AppendText($"클라이언트: {message}\n");
            textBox3.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DisconnectClient();
        }

        private void DisconnectClient()
        {
            if (client != null)
            {
                try
                {
                    stream?.Close(); client.Close();
                }
                catch { }
                client = null;
                richTextBox1.AppendText("클라이언트 접속 종료 우하하\n");
            }
        }

        //종료 버튼
        private void button4_Click(object sender, EventArgs e)
        {
            DisconnectClient();
            Application.Exit();
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectClient();
        }
    }
}