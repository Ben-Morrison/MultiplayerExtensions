using OpusDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.VOIP
{
    public class VoipListener : ITickable
    {
        private OpusEncoder? _encoder;

        private AudioClip? recording;
        private float[]? recordingBuffer;
        //private float[]? resampleBuffer;
        private string? _usedMicrophone;
        private int lastPos = 0;
        private int index;
        public int inputFreq;
        public event EventHandler<VoipPacket>? OnAudioGenerated;

        public bool SetMicrophone(string? deviceName)
        {
            bool success = false;
            if (Microphone.devices.Contains(deviceName))
            {
                success = true;
                _usedMicrophone = deviceName;
            }
            return success;
        }

        private bool _isListening;

        public bool IsListening
        {
            get
            {
                return _isListening;
            }
            set
            {
                if (!_isListening && value && recordingBuffer != null)
                {
                    index += 3;
                    lastPos = Math.Max(Microphone.GetPosition(_usedMicrophone) - recordingBuffer.Length,
                                        0);
                }
                _isListening = value;
            }
        }



        public void StartRecording()
        {
            if (Microphone.devices.Length == 0)
                return;
            recording = Microphone.Start(_usedMicrophone, true, 20, AudioUtils.GetFreqForMic(_usedMicrophone));
        }

        protected OpusEncoder GetEncoder(string? deviceName, int sampleRate)
        {
            if (_encoder != null)
            {
                _encoder.Dispose();
                _encoder = null;
            }
            inputFreq = AudioUtils.GetFreqForMic(deviceName);
            _encoder = new OpusEncoder(OpusDotNet.Application.VoIP, inputFreq, 1)
            {
                Bitrate = 128000,
            };
            return _encoder;
        }
        public void StopRecording()
        {
            Microphone.End(_usedMicrophone);
            if (recording != null)
                GameObject.Destroy(recording);
            recording = null;
        }

        public void Tick()
        {
            if (recording == null)
                return;
            int now = Microphone.GetPosition(_usedMicrophone);
            int length = now - lastPos;
            if(now < lastPos)
            {
                lastPos = 0;
                length = now;
            }
            while(length >= recordingBuffer.Length)
            {
                if (_isListening && recording.GetData(recordingBuffer, lastPos))
                {
                    //Send..
                    index++;
                    if (OnAudioGenerated != null)
                    {
                        ////Downsample if needed.
                        //if (recordingBuffer != resampleBuffer)
                        //{
                        //    AudioUtils.Resample(recordingBuffer, resampleBuffer, inputFreq, AudioUtils.GetFrequency(encoder.mode));
                        //}
                        byte[] pcmBytes = new byte[recordingBuffer.Length * 2];
                        AudioUtils.Convert(recordingBuffer, ref pcmBytes);
                        int pcmLength = 0;
                        var data = _encoder.Encode(pcmBytes, pcmBytes.Length, out pcmLength);

                        VoipPacket frag = VoipPacket.Create("test", index, data);

                        OnAudioGenerated?.Invoke(this, frag);
                    }
                }
                length -= recordingBuffer.Length;
                lastPos += recordingBuffer.Length;
            }
        }
    }
}
