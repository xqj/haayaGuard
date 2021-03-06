﻿using AForge.Video.DirectShow;
using Haaya.GuardClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GuardClient_Win
{
    public partial class Form1 : Form
    {
        private ConcurrentQueue<String> _statusQueue = new ConcurrentQueue<String>();
        private FilterInfoCollection videoDevices;
        public Form1()
        {
            InitializeComponent();
            
        }
        public void Log(string content)
        {
            _statusQueue.Enqueue(content);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 枚举所有视频输入设备
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo device in videoDevices)
                {
                    tscbxCameras.Items.Add(device.Name);
                }

                tscbxCameras.SelectedIndex = 0;
            }
            catch (ApplicationException)
            {
                tscbxCameras.Items.Add("No local capture devices");
                videoDevices = null;
            }
        }
        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            ServiceImp.Instance.WriteImage(image);
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Safe.key = secretKey.Text;
            ServiceImp.Instance.Init();
            CameraConn();
            
        }

        private void CameraConn()
        {
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[tscbxCameras.SelectedIndex].MonikerString);
            videPlayer.VideoSource = videoSource;
            videPlayer.Start();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ServiceImp.Instance.Stop();
            videPlayer.SignalToStop();
            videPlayer.WaitForStop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            toolStripButton2_Click(null, null);
            ServiceImp.Instance.Clear();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int count = _statusQueue.Count;
            string content = string.Empty;
            for (var i = 0; i < count; i++)
            {
                _statusQueue.TryDequeue(out content);
                richState.AppendText(content + "\n");
            }
        }
    }
}
