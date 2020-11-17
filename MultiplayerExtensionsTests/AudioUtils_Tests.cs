using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiplayerExtensions.VOIP;
using MultiplayerExtensions.VOIP.Utilities;

namespace MultiplayerExtensionsTests
{
    [TestClass]
    public class AudioUtils_Tests
    {
        [TestMethod]
        public void FloatAryToBytes()
        {
            float[] original = new float[] { .434f, .23432f, .2342f, .9349f };
            byte[] byteAry = new byte[original.Length * 2];

            AudioUtils.Convert(original, original.Length, byteAry);

            float[] reverted = new float[original.Length];
            AudioUtils.Convert(byteAry, byteAry.Length, reverted);

            for(int i = 0; i < original.Length; i++)
            {
                Console.WriteLine($"{original[i]} -> {byteAry[i].ToString("X2")}{byteAry[i+1].ToString("X2")} -> {reverted[i]}");
            }
            for (int i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(Math.Round(original[i], 3), Math.Round(reverted[i], 3), $"Mismatch at '{i}', expected '{original[i]}' but got '{reverted[i]}'");
            }
        }
        [TestMethod]
        public void ByteAryToFloats()
        {
            byte[] original = new byte[] { 0x37, 0x8C, 0x37, 0x8C };
            float[] floatAry = new float[original.Length / 2];
            AudioUtils.Convert(original, original.Length, floatAry);

            byte[] reverted = new byte[original.Length];
            AudioUtils.Convert(floatAry, floatAry.Length, reverted);

            for (int i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(original[i], reverted[i]);
            }
        }
    }
}
