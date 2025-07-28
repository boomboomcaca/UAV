using Magneto.Contract.Audio;
using NUnit.Framework;

namespace Tests.Contract.Algorithm;

public class AudioProcessTest
{
    [Test]
    [Order(1)]
    public void Test()
    {
        new AudioProcess().Test();
    }
}