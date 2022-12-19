using UKMCAB.Common.Security.Tokens;

namespace UKMCAB.Test.Common;

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

        Assert.That(data2, Is.EqualTo(data));
    }
}