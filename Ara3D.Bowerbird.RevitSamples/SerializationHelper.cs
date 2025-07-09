using System;
using System.Diagnostics;
using System.IO;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.RevitSamples;

public class SerializationHelper
{
    public SerializationHelper(string method, string fileName, Action<Stream> serializeAction)
    {
        Method = method;
        FileName = fileName;
        var sw = Stopwatch.StartNew();
        using var fs = File.Create(OutputFilePath);
        try
        {
            serializeAction(fs);
            FileSize = PathUtil.GetFileSizeAsString(OutputFilePath);
        }
        catch (Exception e)
        {
            Exception = e;
            FileSize = "ERROR";
        }
        Elapsed = sw.Elapsed;
    }

    public string Method;
    public string FileName;
    public FilePath OutputFilePath => Path.Combine(Path.GetTempPath(), FileName);
    public TimeSpan Elapsed;
    public string FileSize;
    public Exception? Exception;
}