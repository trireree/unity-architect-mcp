interface BenchmarkResult {
    metric: string;
    legacyMcp: string | number;
    architectMcp: string | number;
    improvement: string;
}
export declare function runBenchmarks(): Promise<BenchmarkResult[]>;
export {};
