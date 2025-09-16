using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Threading;

namespace RTSP
{
    public partial class Form1 : Form
    {
        //RTSP 영상을 가져오는 객체
        private VideoCapture _capture;
        private bool _isRunning = false;

        //원본 영상 비트맵
        private Bitmap bmpOriginal;
        //객체 탐지 영상 비트맵
        private Bitmap bmpProcessed;

        //ONNX Runtime 세션(yolov8n 실행)
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

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load; //폼 시작 이벤트
            this.FormClosing += Form1_FormClosing; //폼 종료 이벤트
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //pictureBox에 RTSP 영상 딱 맞도록~~
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            //RTSP주소
            string rtspUrl = "rtsp://210.99.70.120:1935/live/cctv007.stream";
            _capture = new VideoCapture(rtspUrl);

            //스레드 수 제한해서 CPU 점유율 조절함!!
            var options = new SessionOptions();
            options.IntraOpNumThreads = 1;//코어 개수 제한해서 성능 높이깅~~

            //yolov8n 모델 로드
            _onnxSession = new InferenceSession("yolov8n.onnx", options);

            _isRunning = true;
            Task.Run(() => ProcessVideo()); //영상 처리 시작!!!
        }

        private async void ProcessVideo()
        {
            int targetFPS = 25; //FPS
            int frameDelay = 1000 / targetFPS; //프레임 하나당 대기 시간

            while (_isRunning)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                using var frame = _capture.QueryFrame();//RTSP 영상 프레임 로드
                if (frame != null)
                {
                    //원본 영상(pictureBox1)
                    bmpOriginal = UpdateBitmap(bmpOriginal, frame, pictureBox1);

                    //객체 탐지 스타투~~
                    using var processedFrame = frame.Clone();
                    RunYoloOnnx(processedFrame);

                    //탐지 결과 영상(pictureBox2)
                    bmpProcessed = UpdateBitmap(bmpProcessed, processedFrame, pictureBox2);
                }

                sw.Stop();
                //처리 속도에 따른 FPS 조절~~
                int sleepTime = frameDelay - (int)sw.ElapsedMilliseconds;
                if (sleepTime > 0)
                    await Task.Delay(sleepTime);
            }
        }

        private Bitmap UpdateBitmap(Bitmap bmp, Mat mat, PictureBox pb)
        {
            bmp?.Dispose(); //이전 Bitmap 메모리 해제
            bmp = mat.ToBitmap();

            if (pb.InvokeRequired)
                pb.Invoke(new Action(() => pb.Image = bmp));
            else
                pb.Image = bmp;

            return bmp;
        }

        //onnx 추론 시작합니둥
        private void RunYoloOnnx(Mat frameMat)
        {
            int inputSize = 640; 
            int origWidth = frameMat.Width;
            int origHeight = frameMat.Height;

            //프레임 크기 640x640 리사이즈
            using var resizedMat = new Mat();
            CvInvoke.Resize(frameMat, resizedMat, new Size(inputSize, inputSize));

            //정규화
            resizedMat.ConvertTo(resizedMat, Emgu.CV.CvEnum.DepthType.Cv32F, 1.0 / 255);

            //yolo 입력 배열 준비
            float[] inputData = new float[1 * 3 * inputSize * inputSize];
            var channels = resizedMat.Split();
            try
            {
                //BGR → CHW 데이터 바꿈
                for (int c = 0; c < 3; c++)
                {
                    float[] channelData = new float[inputSize * inputSize];
                    System.Runtime.InteropServices.Marshal.Copy(channels[c].DataPointer, channelData, 0, inputSize * inputSize);
                    Array.Copy(channelData, 0, inputData, c * inputSize * inputSize, inputSize * inputSize);
                }
            }
            finally
            {
                foreach (var ch in channels) ch.Dispose();
            }

            //onnx 입력 텐서 스타투~~
            var inputTensor = new DenseTensor<float>(inputData, new int[] { 1, 3, inputSize, inputSize });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };

            //yolo 실행!!!
            using var results = _onnxSession.Run(inputs);
            var outputTensor = results.First().AsTensor<float>();

            int numClasses = _classNames.Length;
            int numBoxes = outputTensor.Dimensions[2]; //예측 박스 개수

            List<RectangleF> boxes = new List<RectangleF>();
            List<float> scores = new List<float>();
            List<int> classIds = new List<int>();

            for (int i = 0; i < numBoxes; i++)
            {
                float cx = outputTensor[0, 0, i];
                float cy = outputTensor[0, 1, i];
                float w = outputTensor[0, 2, i];
                float h = outputTensor[0, 3, i];

                float[] classScores = new float[numClasses];
                for (int c = 0; c < numClasses; c++)
                    classScores[c] = outputTensor[0, 4 + c, i];

                int classId = Array.IndexOf(classScores, classScores.Max());
                float conf = classScores[classId]; //최고 점수

                //임계값 이하 무시하깅~~ (메모리 점유율 낮아짐!!!)
                if (conf < 0.3f) continue;

                float x = (cx - w / 2) * origWidth / inputSize;
                float y = (cy - h / 2) * origHeight / inputSize;
                float width = w * origWidth / inputSize;
                float height = h * origHeight / inputSize;

                boxes.Add(new RectangleF(x, y, width, height));
                scores.Add(conf);
                classIds.Add(classId);
            }

            //NMS (중복 박스 최대한 제거.. 바운딩 박스 폭주함)
            int[] keep = Nms(boxes.ToArray(), scores.ToArray(), 0.4f);

            //최종 바운딩 박스 그리깅
            for (int i = 0; i < keep.Length; i++)
            {
                int idx = keep[i];
                var box = boxes[idx];

                CvInvoke.Rectangle(frameMat, Rectangle.Round(box), new MCvScalar(0, 0, 255), 2);

                //클래스명+신뢰도 표시
                if (classIds[idx] >= 0 && classIds[idx] < _classNames.Length)
                {
                    CvInvoke.PutText(frameMat,
                        $"{_classNames[classIds[idx]]} {scores[idx]:0.00}",
                        new Point((int)box.X, (int)Math.Max(0, box.Y - 5)),
                        Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5,
                        new MCvScalar(255, 255, 255), 1);
                }
            }
        }

        // NMS (Non-Maximum Suppression) 알고리즘 (비최대 억제)
        private int[] Nms(RectangleF[] boxes, float[] scores, float iouThreshold)
        {
            List<int> indices = new List<int>();

            //점수 순대로 정렬
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

            float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            float union = a.Width * a.Height + b.Width * b.Height - intersection;
            return union <= 0 ? 0 : intersection / union;
        }

        //폼 종료시 자원 정리~~
        //메모리 누수, 성능 등 문제 해결되기때문에 해주는 것이 좋음!
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isRunning = false; //영상 처리 루프 종료
            _capture?.Dispose(); //영상 로드 해제
            _onnxSession?.Dispose(); //onnx 추론 해제
            bmpOriginal?.Dispose();
            bmpProcessed?.Dispose();
        }
    }
}
