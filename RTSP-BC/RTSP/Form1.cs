using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTSP
{
    public partial class Form1 : Form
    {
        //RTSP 읽어오기
        private VideoCapture _capture;
        //영상 처리 루프 제어 플래그
        private bool _isRunning = false;

        //고정 Bitmap 할당
        private Bitmap bmpOriginal; //영상 원본
        private Bitmap bmpProcessed;  //개선 영상

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //디자인에서 설정한 화면에 맞게 카메라 크기 조정하깅~~~
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            //RTSP 영상 URL
            string rtspUrl = "rtsp://210.99.70.120:1935/live/cctv007.stream"; //ID+PW(X)
            //"rtsp://아이디:비밀번호@아이피:포트번호/채널" //ID+PW(O)

            //스트리밍 객체 준비!!!
            _capture = new VideoCapture(rtspUrl);
            _isRunning = true; //스타투

            //비동기!!!
            Task.Run(() => ProcessVideo());
        }

        private void ProcessVideo()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int targetFPS = 30; //목표 FPS
            double frameInterval = 1000.0 / targetFPS;
            long lastUpdateTime = 0;

            while (_isRunning)//폼 닫힐 때까지 반복
            {
                double elapsed = sw.ElapsedMilliseconds;

                //BGR 형식으로 영상 프레임 읽음!
                using (var frame = _capture.QueryFrame()?.ToImage<Bgr, byte>())
                {
                    if (frame != null) //정상적으로 읽혔을 때
                    {
                        using (var original = frame.Clone())
                        {
                            var processed = frame.Clone();

                            //1.샤프닝 (선명도+윤곽선 강조)
                            using (var blurred = processed.SmoothGaussian(0, 0, 5, 5))
                            {
                                CvInvoke.AddWeighted(processed, 2.0, blurred, -1.0, 0, processed); //선명도 2배
                            }

                            //2.감마 조정 (밝기)
                            double gamma = 1.2; //gamma>1.0 (밝아짐~~), 약간 밝게
                            byte[] lutData = new byte[256];
                            for (int i = 0; i < 256; i++)
                                lutData[i] = (byte)Math.Min(255, (int)(Math.Pow(i / 255.0, 1.0 / gamma) * 255.0));

                            using (var lut = new Mat(1, 256, DepthType.Cv8U, 1))
                            {
                                lut.SetTo<byte>(lutData);
                                CvInvoke.LUT(processed, lut, processed);
                            }

                            //3.채도 조정
                            using (var hsv = processed.Convert<Hsv, byte>()) //HSV로 변경
                            {
                                var channels = hsv.Split();
                                channels[1]._Mul(1.2); //채도 1.2배

                                using (var vm = new VectorOfMat())
                                {
                                    foreach (var c in channels)
                                        vm.Push(c.Mat);
                                    CvInvoke.Merge(vm, hsv);
                                }

                                foreach (var c in channels) c.Dispose();
                                CvInvoke.CvtColor(hsv, processed, ColorConversion.Hsv2Bgr); //다시 BGR로 변경
                            }

                            //4.UI 업데이트 -> pictureBox
                            //고정 Bitmap으로 메모리 누수 최소화
                            bmpOriginal = UpdateBitmap(bmpOriginal, original, pictureBox1);
                            bmpProcessed = UpdateBitmap(bmpProcessed, processed, pictureBox2);
                            lastUpdateTime = (long)elapsed;

                            processed.Dispose(); //Mat 해제해서 메모리 누수 감소시키깅~~
                        }
                    }
                }
                //Sleep 걸어서 CPU 점유 낮춤!
                System.Threading.Thread.Sleep(1);
            }
        }

        //Bitmap 덮어쓰기 함수
        private Bitmap UpdateBitmap(Bitmap bmp, Image<Bgr, byte> img, PictureBox pb)
        {
            if (bmp == null)
            {
                bmp = img.ToBitmap();
                if (pb.InvokeRequired)
                    pb.Invoke(new Action(() => pb.Image = bmp));
                else
                    pb.Image = bmp;
            }
            else
            {
                //기존 Bitmap에 영상 덮어쓰기
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.WriteOnly,
                                               PixelFormat.Format24bppRgb);

                Marshal.Copy(img.Bytes, 0, data.Scan0, img.Bytes.Length);
                bmp.UnlockBits(data);

                //ui 갱신~~~
                if (pb.InvokeRequired)
                    pb.Invoke(new Action(() => pb.Refresh()));
                else
                    pb.Refresh();
            }

            return bmp;
        }

        //폼 끌 때 영상 루프, Bitmap, videocapture 해제됨!!!
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isRunning = false;
            _capture?.Dispose();

            bmpOriginal?.Dispose();
            bmpProcessed?.Dispose();
        }
    }
}
