using System.Collections.Generic;

namespace Ara3D.Bowerbird.RevitSamples;

public class NaiveParameters
{
    public Dictionary<long, Dictionary<string, string>> ElementParameters { get; set; }= new ();
}