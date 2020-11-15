using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.VOIP
{
    public enum BandMode
    {
        Narrow,
        Medium,
        Wide,
        SuperWide,
        Full
    }
    //https://github.com/DwayneBull/UnityVOIP/blob/master/AudioUtils.cs
    public static class AudioUtils
    {
        public static void ApplyGain(float[] samples, float gain)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= gain;
            }
        }

        public static float GetMaxAmplitude(float[] samples)
        {
            return samples.Max();
        }

        public static int GetFrequency(BandMode mode)
        {
            return mode switch
            {
                BandMode.Narrow => 8000,
                BandMode.Medium => 12000,
                BandMode.Wide => 16000,
                BandMode.SuperWide => 24000,
                BandMode.Full => 48000,
                _ => 16000
            };
        }

        public static void Resample(float[] source, float[] target, int inputSampleRate, int outputSampleRate, int outputChannelsNum = 1)
        {
            Resample(source, target, source.Length, target.Length, inputSampleRate, outputSampleRate, outputChannelsNum);
        }

        public static void Resample(float[] source, float[] target, int inputNum, int outputNum, int inputSampleRate, int outputSampleRate, int outputChannelsNum)
        {
            float ratio = inputSampleRate / (float)outputSampleRate;
            if (ratio % 1f <= float.Epsilon)
            {
                int intRatio = Mathf.RoundToInt(ratio);
                for (int i = 0; i < (outputNum / outputChannelsNum) && (i * intRatio) < inputNum; i++)
                {
                    for (int j = 0; j < outputChannelsNum; j++)
                        target[i * outputChannelsNum + j] = source[i * intRatio];
                }
            }
            else
            {
                if (ratio > 1f)
                {
                    for (int i = 0; i < (outputNum / outputChannelsNum) && Mathf.CeilToInt(i * ratio) < inputNum; i++)
                    {
                        for (int j = 0; j < outputChannelsNum; j++)
                            target[i * outputChannelsNum + j] = Mathf.Lerp(source[Mathf.FloorToInt(i * ratio)], source[Mathf.CeilToInt(i * ratio)], ratio % 1);
                    }
                }
                else
                {
                    for (int i = 0; i < (outputNum / outputChannelsNum) && Mathf.FloorToInt(i * ratio) < inputNum; i++)
                    {
                        for (int j = 0; j < outputChannelsNum; j++)
                        {
                            target[i * outputChannelsNum + j] = source[Mathf.FloorToInt(i * ratio)];
                        }
                    }
                }
            }
        }

        public static int GetFreqForMic(string? deviceName = null)
        {
            int minFreq;
            int maxFreq;

            Microphone.GetDeviceCaps(deviceName, out minFreq, out maxFreq);

            if (minFreq >= 12000)
            {
                if (FindClosestFreq(minFreq, maxFreq) != 0)
                {
                    return FindClosestFreq(minFreq, maxFreq);
                }
                else
                {
                    return minFreq;
                }
            }
            else
            {
                return maxFreq;
            }
        }

        public static int[] possibleSampleRates = new int[] { 8000, 12000, 16000, 24000, 48000 };

        public static int FindClosestFreq(int minFreq, int maxFreq)
        {
            foreach (int sampleRate in possibleSampleRates)
            {
                if (sampleRate >= minFreq && sampleRate <= maxFreq)
                {
                    return sampleRate;
                }
            }
            return 0;
        }

        public static void Convert(short[] data, ref float[] target)
        {
            for (int i = 0; i < data.Length; i++)
            {
                target[i] = data[i] / (float)short.MaxValue;
            }
        }

        public static void Convert(byte[] data, ref short[] target)
        {
            for(int i = 0; i < data.Length; i = i + 2)
            {
                target[i] = (short)((data[i + 1] << 8) | data[i]);
            }
        }

        public static void Convert(float[] data, ref short[] target)
        {
            for (int i = 0; i < data.Length; i++)
            {
                target[i] = (short)(data[i] * short.MaxValue);
            }
        }

        public static void Convert(byte[] data, ref float[] target)
        {
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = ((short)((data[i*2 + 1] << 8) | data[i*2])) / (float)short.MaxValue;
            }
        }

        public static void Convert(float[] data, ref byte[] target)
        {
            for (int i = 0; i < data.Length; i++)
            {
                short shortVal = ((short)(data[i] * short.MaxValue));
                byte low = (byte)(shortVal);
                byte high = (byte)((shortVal >> 8) & 0xFF);
                target[i * 2] = low;
                target[i * 2 + 1] = high;
            }
        }
    }
}
