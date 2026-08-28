import { UnityBridgeRequest, UnityMcpResponse } from "./types/unity-schemas.js";
export declare class UnityClient {
    private bridgeUrl;
    constructor(bridgeUrl?: string);
    checkHealth(): Promise<{
        healthy: boolean;
        version?: string;
        error?: string;
    }>;
    execute(request: UnityBridgeRequest): Promise<UnityMcpResponse>;
}
