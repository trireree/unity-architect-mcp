export class UnityClient {
    bridgeUrl;
    constructor(bridgeUrl = process.env.UNITY_BRIDGE_URL || "http://127.0.0.1:8080") {
        this.bridgeUrl = bridgeUrl.replace(/\/$/, "");
    }
    async checkHealth() {
        try {
            const response = await fetch(`${this.bridgeUrl}/health`, {
                method: "GET",
                signal: AbortSignal.timeout(3000),
            });
            if (response.ok) {
                const data = (await response.json());
                return { healthy: true, version: data.data };
            }
            return { healthy: false, error: `HTTP ${response.status}: ${response.statusText}` };
        }
        catch (err) {
            return {
                healthy: false,
                error: `Cannot connect to Unity Bridge at ${this.bridgeUrl}. Please ensure Unity Editor is open and 'Unity MCP Bridge' package is loaded. Details: ${err.message}`,
            };
        }
    }
    async execute(request) {
        try {
            const response = await fetch(`${this.bridgeUrl}/`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(request),
                signal: AbortSignal.timeout(30000), // 30s timeout for complex operations like NavMesh baking or compilation
            });
            const json = (await response.json());
            return json;
        }
        catch (err) {
            if (err.name === "TimeoutError") {
                return {
                    success: false,
                    error: `Action '${request.action}' timed out after 30 seconds. Unity may be compiling or performing heavy computation.`,
                };
            }
            return {
                success: false,
                error: `Communication error with Unity Editor (${this.bridgeUrl}): ${err.message}. Make sure Unity Editor is running and not locked in a modal dialog.`,
            };
        }
    }
}
