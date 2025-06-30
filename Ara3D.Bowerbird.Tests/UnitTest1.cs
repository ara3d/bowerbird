using Ara3D.Bowerbird.RevitSamples;
using MessagePack.Resolvers;
using MessagePack;
using Newtonsoft.Json;

namespace Ara3D.Bowerbird.Tests
{
    public static class Tests
    {
        [Test]
        public static void Test()
        {
            var data = File.ReadAllBytes(@"C:\Users\cdigg\AppData\Local\Temp\254d4ca9-96cc-4359-9b08-95ef06f05d1e\brep.mp");
            var dd = MessagePackSerializer.Typeless.Deserialize(data);
            
            var s = JsonConvert.SerializeObject(dd, Formatting.Indented);
            var outputFilePath = Path.Combine(Path.GetTempPath(), "brep.json");
            File.WriteAllText(outputFilePath, s);
        }
    }
}