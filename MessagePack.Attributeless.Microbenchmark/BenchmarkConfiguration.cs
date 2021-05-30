namespace MessagePack.Attributeless.Microbenchmark
{
    public class BenchmarkConfiguration
    {
        public bool DoIncludeJson { get; set; }
        public bool DontIncludeOthermethods { get; set; }
        public int NumberOfRecords { get; set; } = 100;
        public int Repetitions { get; set; } = 100;
    }
}