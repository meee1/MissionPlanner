using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Logs
{
    /// <summary>
    /// Regression tests for <see cref="FFT2.FreqTable"/>, the frequency-bin
    /// mapping used by the vibration/FFT log analysis.
    /// </summary>
    [TestClass]
    public class FftTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void FreqTable_HasHalfSampleCountBins()
        {
            var freqs = new FFT2().FreqTable(samplecount: 1024, samplerate: 1000);
            Assert.AreEqual(512, freqs.Length); // N/2 bins
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FreqTable_BinFrequencies_AreLinear()
        {
            // bin i frequency = i * samplerate / N
            var freqs = new FFT2().FreqTable(samplecount: 8, samplerate: 1000);
            // N/2 = 4 bins: 0, 125, 250, 375
            CollectionAssert.AreEqual(new double[] { 0, 125, 250, 375 }, freqs);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FreqTable_FirstBinIsDc_LastBelowNyquist()
        {
            var freqs = new FFT2().FreqTable(samplecount: 16, samplerate: 1600);
            Assert.AreEqual(0.0, freqs[0], 1e-9);                 // DC
            Assert.IsTrue(freqs[freqs.Length - 1] < 1600 / 2.0);  // below Nyquist
        }
    }
}
