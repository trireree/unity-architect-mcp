interface TestReport {
    testName: string;
    category: string;
    status: "CODE-LEVEL PASS" | "REAL UNITY PASS" | "NOT VERIFIED";
    durationMs: number;
    details: string;
}
export declare function runFullTestSuite(): Promise<{
    tests: TestReport[];
    benchmarks: any[];
}>;
export {};
