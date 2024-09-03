using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;


namespace Audio_Y_video
{
    public partial class Form1 : Form
    {
        private VideoCaptureDevice videoSource;
        private WaveInEvent waveIn;
        private BufferedWaveProvider bufferedWaveProvider;
        public Form1()
        {
            InitializeComponent();
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            InitializeVideo();
            InitializeAudio();
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void InitializeVideo()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
        }
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
           
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        private void InitializeAudio()
        {
            waveIn = new WaveInEvent();

            // Configura la frecuencia de muestreo y el número de canales
            waveIn.WaveFormat = new WaveFormat(22050, 1); // Frecuencia de muestreo de 22.05kHz, Mono

            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                BufferLength = 65536 // Tamaño del búfer en bytes (ej. 64KB)
            };

            // Configura para descartar datos en exceso cuando el búfer esté lleno
            bufferedWaveProvider.DiscardOnBufferOverflow = true;

            waveIn.DataAvailable += WaveIn_DataAvailable;
        }
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                // Añadir datos al búfer
                bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
            catch (InvalidOperationException ex)
            {
                // Manejar el error de buffer lleno aquí
                Console.WriteLine("Error al añadir datos al búfer: " + ex.Message);
            }

        }
        private bool isRecordingAudio = false;

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (videoSource != null && !videoSource.IsRunning)
            {
                videoSource.Start(); // Iniciar la captura de video
                btnStart.Enabled = false; // Deshabilitar el botón Start
                btnStop.Enabled = true; // Habilitar el botón Stop
            }

            // Verificar si el audio ya está grabando usando el indicador
            if (!isRecordingAudio)
            {
                waveIn.StartRecording(); // Iniciar la captura de audio
                isRecordingAudio = true; // Actualizar el estado de grabación
                btnStart.Enabled = false; // Deshabilitar el botón Start
                btnStop.Enabled = true; // Habilitar el botón Stop
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.Stop(); // Detener la captura de video
            }

            // Detener la captura de audio si está grabando usando el indicador
            if (isRecordingAudio)
            {
                waveIn.StopRecording(); // Detener la captura de audio
                isRecordingAudio = false; // Actualizar el estado de grabación
            }

            btnStart.Enabled = true; // Habilitar el botón Start
            btnStop.Enabled = false; // Deshabilitar el botón Stop
        }
        protected override void OnClosed(EventArgs e)
        {
            if (videoSource.IsRunning)
                videoSource.Stop();
            waveIn.Dispose();
            base.OnClosed(e);
        }
        




    }
}
