using System;
using NUnit.Framework;
using UKMCAB.Common.Security.Tokens;
using NUnit.Framework.Legacy;

namespace UKMCAB.Common.Tests;

[TestFixture]
public class SecureTokenProcessorTest
{
    [Test]
    public void TestEncryptDecrypt()
    {
        var data = new Tuple<int, int>(10, 50);

        var keyiv = CryptoHelper.GenerateKeyIV();
        var stp = new SecureTokenProcessor(keyiv.ToString());
        var token = stp.Enclose(data);
        var data2 = stp.Disclose<Tuple<int, int>>(token);

        ClassicAssert.That(data2, Is.EqualTo(data));
    }
}