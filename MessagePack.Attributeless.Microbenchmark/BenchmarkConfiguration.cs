namespace MessagePack.Attributeless.Microbenchmark
{
    public class BenchmarkConfiguration
    {
        public string CsvPath { get; set; } = "MessagePack.Attributeless.Benchmark.csv";
        public bool DoIncludeJson { get; set; }
        public bool DontIncludeOthermethods { get; set; }
        public int NumberOfRecords { get; set; } = 100;
        public int Repetitions { get; set; } = 100;
    }
}