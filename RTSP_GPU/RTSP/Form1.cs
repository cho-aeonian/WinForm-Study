using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors; //ONNX Tensor 처리

namespace RTSP
{
    //RTSP 영상+yolo ONNX 객체 탐지
    public partial class Form1 : Form
    {
        private VideoCapture _capture; //RTSP 영상 실시간 캡처
        private bool _isRunning = false; //영상 처리 루프 제어 플래그
        private Bitmap bmpOriginal;   //원본 영상
        private Bitmap bmpProcessed;  //탐지 영상
        private InferenceSession _onnxSession;

        private readonly string[] _classNames = new string[]
        {
            "person","bicycle","car","motorcycle","airplane","bus","train","truck","boat","traffic light",
            "fire hydrant","stop sign","parking meter","bench","bird","cat","dog","horse","sheep","cow",
            "elephant","bear","zebra","giraffe","backpack","umbrella","handbag","tie","suitcase","frisbee",
            "skis","snowboard","sports ball","kite","baseball bat","baseball glove","skateboard",
            "surfboard","tennis racket","bottle","wine glass","cup","fork","knife","spoon","bowl",
            "banana","apple","sandwich","orange","broccoli","carrot","hot dog","pizza","donut","cake",
            "chair","couch","potted plant","bed","dining table","toilet","tv","laptop","mouse","remote",
            "keyboard","cell phone","microwave","oven","toaster","sink","refrigerator","book","clock",
            "vase","scissors","teddy bear","hair drier","toothbrush"
        };

        //최신 프레임만 유지하도록!!(메모리 폭증, 영상 속도 지연 문제 해결)
        private Mat latestFrame = null;

        //최신 프레임 접근할 때 동기화시키도록~~
        private object frameLock = new object();

        public Form1()
        {
            InitializeComponent();

            //DoubleBuffer 활성화 (폼 실행 시 깜빡임 최소화)
            //UserPaint + AllPaintingInWmPaint + OptimizedDoubleBuffer
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //각 영상 비율 유지
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            //RTSP 주소
            string rtspUrl = "rtsp://210.99.70.120:1935/live/cctv007.stream";
            _capture = new VideoCapture(rtspUrl);
            //캡처 FPS 설정
            _capture.Set(Emgu.CV.CvEnum.CapProp.Fps, 30);

            //ONNX Runtime GPU 사용
            var options = new SessionOptions();
            options.AppendExecutionProvider_CUDA(0);
            _onnxSession = new InferenceSession("yolov8n.onnx", options); //yolo 모델 로드

            _isRunning = true; //루프 시작 플래그

            //프레임 캡처+처리 쓰레드 시작!!!
            StartCaptureLoop();
            StartProcessingLoop();
        }

        //RTSP 프레임 캡처(비동기, 최신 프레임만 유지하도록!!!)
        private void StartCaptureLoop()
        {
            Task.Run(() =>
            {
                while (_isRunning)
                {
                    ///RTSP에서 프레임 읽어 오기
                    using var frame = _capture.QueryFrame();
                    if (frame != null)
                    {
                        lock (frameLock)
                        {
                            //이전 프레임 해제!!
                            latestFrame?.Dispose();

                            //새 프레임 복사 → 최신 프레임 갱신
                            latestFrame = frame.Clone();
                        }
                    }
                    Thread.Sleep(1); //CPU 점유율 최소화!!!!
                }
            });
        }

        //프레임 처리 루프 (비동기)
        //최신 프레임만 처리 → 영상 지연 최소화하깅~~~
        private void StartProcessingLoop()
        {
            Task.Run(async () =>
            {
                int targetFPS = 30; //FPS
                int frameDelay = 1000 / targetFPS; //프레임 간 지연

                while (_isRunning)
                {
                    Mat frameToProcess = null;

                    lock (frameLock)
                    {
                        //최신 프레임 가져오기
                        if (latestFrame != null)
                        {
                            frameToProcess = latestFrame;
                            latestFrame = null; //이전 프레임 초기화
                        }
                    }

                    if (frameToProcess != null)
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew(); //처리 시간 측정

                        //원본 영상 출력
                        UpdatePictureBox(pictureBox1, frameToProcess);

                        //yolo 객체 탐지 실행
                        RunYoloOnnx(frameToProcess);
                        //탐지 영상 출력
                        UpdatePictureBox(pictureBox2, frameToProcess);

                        frameToProcess.Dispose(); //처리 된 프레임 해제

                        sw.Stop();
                        int sleepTime = frameDelay - (int)sw.ElapsedMilliseconds;
                        if (sleepTime > 0)
                            await Task.Delay(sleepTime); //FPS 조절
                    }
                    else
                    {
                        await Task.Delay(1); //프레임 없으면 최소한으로만 대기!
                    }
                }
            });
        }

        //UI 쓰레드 안전(pictureBox 업데이트)
        //Clone + Dispose → 폼 실행 시 깜빡임 최소화
        private void UpdatePictureBox(PictureBox pb, Mat mat)
        {
            if (pb.InvokeRequired)
            {
                //UI 쓰레드 호출
                pb.Invoke(new Action(() => DrawMat(pb, mat)));
            }
            else
            {
                DrawMat(pb, mat); //직접 호출도 가능함!!!
            }
        }

        //Mat → Bitmap 변환 후 영상 출력
        private void DrawMat(PictureBox pb, Mat mat)
        {
            using (var bmp = mat.ToBitmap())
            {
                if (pb.Image != null)
                {
                    var oldBmp = pb.Image;
                    pb.Image = null;
                    oldBmp.Dispose(); //이전 Bitmap 해제
                }

                pb.Image = (Bitmap)bmp.Clone(); //Clone 후에 할당~~
            }
        }

        //yolo ONNX 추론
        private void RunYoloOnnx(Mat frameMat)
        {
            int inputSize = 640; //yolo 입력 크기
            int origWidth = frameMat.Width;
            int origHeight = frameMat.Height;

            //리사이즈+정규화
            using var resizedMat = new Mat();
            CvInvoke.Resize(frameMat, resizedMat, new Size(inputSize, inputSize));
            resizedMat.ConvertTo(resizedMat, Emgu.CV.CvEnum.DepthType.Cv32F, 1.0 / 255);

            //yolo 입력 데이터 배열 생성
            float[] inputData = new float[1 * 3 * inputSize * inputSize];
            var channels = resizedMat.Split();
            try
            {
                for (int c = 0; c < 3; c++)
                {
                    float[] channelData = new float[inputSize * inputSize];
                    //Mat 데이터를 배열로 복사
                    System.Runtime.InteropServices.Marshal.Copy(channels[c].DataPointer, channelData, 0, inputSize * inputSize);
                    //CHW 순서로 배열 채우기
                    Array.Copy(channelData, 0, inputData, c * inputSize * inputSize, inputSize * inputSize);
                }
            }
            finally
            {
                foreach (var ch in channels) ch.Dispose();
            }

            //onnx 입력 텐서 생성!!!
            var inputTensor = new DenseTensor<float>(inputData, new int[] { 1, 3, inputSize, inputSize });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };

            //onnx 추론~~~
            using var results = _onnxSession.Run(inputs);
            var outputTensor = results.First().AsTensor<float>(); //yolo 출력!!

            int numClasses = _classNames.Length;

            //예측 박수 개수
            int numBoxes = outputTensor.Dimensions[2];

            //박스, 점수, 클래스 저장 리스트
            List<RectangleF> boxes = new List<RectangleF>();
            List<float> scores = new List<float>();
            List<int> classIds = new List<int>();

            // yolo 출력 파싱
            for (int i = 0; i < numBoxes; i++)
            {
                float cx = outputTensor[0, 0, i]; //중심 X
                float cy = outputTensor[0, 1, i]; //중심 Y
                float w = outputTensor[0, 2, i];  //폭
                float h = outputTensor[0, 3, i];  //높이

                float[] classScores = new float[numClasses];
                for (int c = 0; c < numClasses; c++)
                    classScores[c] = outputTensor[0, 4 + c, i]; //클래스 점수

                int classId = Array.IndexOf(classScores, classScores.Max()); //최고 점수 클래스
                float conf = classScores[classId];
                if (conf < 0.3f) continue; //임계값 미만은 무시 (메모리 할당 최소화)

                //원본 크기로 박스 변환
                float x = (cx - w / 2) * origWidth / inputSize;
                float y = (cy - h / 2) * origHeight / inputSize;
                float width = w * origWidth / inputSize;
                float height = h * origHeight / inputSize;

                boxes.Add(new RectangleF(x, y, width, height));
                scores.Add(conf);
                classIds.Add(classId);
            }

            //NMS 적용 (중복 박스 제거 -> 바운딩 박스 폭주 막음!!!)
            int[] keep = Nms(boxes.ToArray(), scores.ToArray(), 0.4f);

            //최종 박스 출력
            for (int i = 0; i < keep.Length; i++)
            {
                int idx = keep[i];
                var box = boxes[idx];

                CvInvoke.Rectangle(frameMat, Rectangle.Round(box), new MCvScalar(0, 0, 255), 2); // 빨간색 박스

                //클래스명+신뢰도 바운딩 박스에 표시
                if (classIds[idx] >= 0 && classIds[idx] < _classNames.Length)
                {
                    CvInvoke.PutText(frameMat,
                        $"{_classNames[classIds[idx]]} {scores[idx]:0.00}",
                        new Point((int)box.X, (int)Math.Max(0, box.Y - 5)),
                        Emgu.CV.CvEnum.FontFace.HersheySimplex,
                        0.5,
                        new MCvScalar(255, 255, 255), 1);
                }
            }
        }

        //NMS (Non-Maximum Suppression) 알고리즘 (비최대 억제)
        private int[] Nms(RectangleF[] boxes, float[] scores, float iouThreshold)
        {
            List<int> indices = new List<int>();
            var order = scores.Select((s, i) => new { Score = s, Index = i })
                              .OrderByDescending(x => x.Score)
                              .ToList();

            bool[] suppressed = new bool[boxes.Length];

            for (int _i = 0; _i < order.Count; _i++)
            {
                int i = order[_i].Index;
                if (suppressed[i]) continue;
                indices.Add(i);

                //loU가 임계값 이상이면 없앰!
                for (int _j = _i + 1; _j < order.Count; _j++)
                {
                    int j = order[_j].Index;
                    if (suppressed[j]) continue;
                    if (IoU(boxes[i], boxes[j]) > iouThreshold)
                        suppressed[j] = true;
                }
            }

            return indices.ToArray();
        }

        // IoU 계산 함수 (Intersection over Union)
        // 교집합/합집합 비율:바운딩 박스가 얼마나 겹치는지 파악!
        private float IoU(RectangleF a, RectangleF b)
        {
            float x1 = Math.Max(a.Left, b.Left);
            float y1 = Math.Max(a.Top, b.Top);
            float x2 = Math.Min(a.Right, b.Right);
            float y2 = Math.Min(a.Bottom, b.Bottom);

            //교집합 넓이
            float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);

            //합집합 넓이
            float union = a.Width * a.Height + b.Width * b.Height - intersection;

            return union <= 0 ? 0 : intersection / union; //IoU 계산
        }

        //폼 종료시 자원 정리~~
        //메모리 누수, 성능 등 문제 해결되기때문에 해주는 것이 좋음!
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isRunning = false;       //영상 처리 루프 종료
            _capture?.Dispose();      //영상 로드 해제
            _onnxSession?.Dispose();  //onnx 추론 해제
            bmpOriginal?.Dispose();   //원본 Bitmap 해제
            bmpProcessed?.Dispose();  //탐지(yolo) Bitmap 해제
            latestFrame?.Dispose();   //이전 프레임 해제
        }
    }
}
