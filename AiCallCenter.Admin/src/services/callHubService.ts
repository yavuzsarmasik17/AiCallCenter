import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel
} from "@microsoft/signalr";

export interface RealtimeCallMessage {
    id: string;
    companyId: string;
    callId: string;
    speaker: number;
    content: string;
    spokenAt: string;
    sequenceNumber: number;
    confidenceScore: number | null;
    audioFileUrl: string | null;
}

export interface RealtimeCallStatus {
    companyId: string;
    callId: string;
    status: number;
    changedAt: string;
    description: string | null;
}

type MessageCreatedHandler = (
    message: RealtimeCallMessage
) => void;

type CallStatusChangedHandler = (
    status: RealtimeCallStatus
) => void;

type ReconnectedHandler = () => void;

class CallHubService {
    private connection: HubConnection | null = null;

    private readonly hubUrl =
        "https://localhost:7280/hubs/calls";

    public get connectionState(): HubConnectionState {
        return (
            this.connection?.state ??
            HubConnectionState.Disconnected
        );
    }

    public get isConnected(): boolean {
        return (
            this.connection?.state ===
            HubConnectionState.Connected
        );
    }

    private createConnection(): HubConnection {
        const token =
            localStorage.getItem("accessToken");

        if (!token) {
            throw new Error(
                "SignalR bağlantısı için accessToken bulunamadı."
            );
        }

        return new HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                accessTokenFactory: () => {
                    return (
                        localStorage.getItem(
                            "accessToken"
                        ) ?? ""
                    );
                }
            })
            .withAutomaticReconnect([
                0,
                2000,
                5000,
                10000,
                30000
            ])
            .configureLogging(
                LogLevel.Information
            )
            .build();
    }

    public async start(): Promise<void> {
        if (
            this.connection?.state ===
            HubConnectionState.Connected
        ) {
            return;
        }

        if (
            this.connection?.state ===
                HubConnectionState.Connecting ||
            this.connection?.state ===
                HubConnectionState.Reconnecting
        ) {
            return;
        }

        if (!this.connection) {
            this.connection =
                this.createConnection();

            this.configureConnectionEvents();
        }

        await this.connection.start();

        console.info(
            "SignalR bağlantısı başarıyla kuruldu."
        );
    }

    public async stop(): Promise<void> {
        if (!this.connection) {
            return;
        }

        if (
            this.connection.state !==
            HubConnectionState.Disconnected
        ) {
            await this.connection.stop();
        }

        this.connection = null;

        console.info(
            "SignalR bağlantısı kapatıldı."
        );
    }

    public async joinCompany(
        companyId: string
    ): Promise<void> {
        this.ensureConnected();

        if (!companyId) {
            throw new Error(
                "Firma kimliği boş olamaz."
            );
        }

        await this.connection!.invoke(
            "JoinCompany",
            companyId
        );

        console.info(
            "Firma SignalR grubuna katılındı:",
            companyId
        );
    }

    public async leaveCompany(
        companyId: string
    ): Promise<void> {
        if (!this.isConnected) {
            return;
        }

        await this.connection!.invoke(
            "LeaveCompany",
            companyId
        );
    }

    public async joinCall(
        callId: string
    ): Promise<void> {
        this.ensureConnected();

        if (!callId) {
            throw new Error(
                "Çağrı kimliği boş olamaz."
            );
        }

        await this.connection!.invoke(
            "JoinCall",
            callId
        );

        console.info(
            "Çağrı SignalR grubuna katılındı:",
            callId
        );
    }

    public async leaveCall(
        callId: string
    ): Promise<void> {
        if (!this.isConnected) {
            return;
        }

        await this.connection!.invoke(
            "LeaveCall",
            callId
        );
    }

    public onMessageCreated(
        handler: MessageCreatedHandler
    ): void {
        this.ensureConnectionCreated();

        this.connection!.on(
            "CallMessageCreated",
            handler
        );
    }

    public offMessageCreated(
        handler: MessageCreatedHandler
    ): void {
        this.connection?.off(
            "CallMessageCreated",
            handler
        );
    }

    public onCallStatusChanged(
        handler: CallStatusChangedHandler
    ): void {
        this.ensureConnectionCreated();

        this.connection!.on(
            "CallStatusChanged",
            handler
        );
    }

    public offCallStatusChanged(
        handler: CallStatusChangedHandler
    ): void {
        this.connection?.off(
            "CallStatusChanged",
            handler
        );
    }

    public onReconnected(
        handler: ReconnectedHandler
    ): void {
        this.ensureConnectionCreated();

        this.connection!.onreconnected(
            handler
        );
    }

    private configureConnectionEvents(): void {
        if (!this.connection) {
            return;
        }

        this.connection.onreconnecting(
            (error) => {
                console.warn(
                    "SignalR bağlantısı yeniden kuruluyor.",
                    error
                );
            }
        );

        this.connection.onreconnected(
            (connectionId) => {
                console.info(
                    "SignalR yeniden bağlandı.",
                    connectionId
                );
            }
        );

        this.connection.onclose(
            (error) => {
                if (error) {
                    console.error(
                        "SignalR bağlantısı hata nedeniyle kapandı.",
                        error
                    );
                } else {
                    console.info(
                        "SignalR bağlantısı kapandı."
                    );
                }
            }
        );
    }

    private ensureConnected(): void {
        if (!this.connection) {
            throw new Error(
                "SignalR bağlantısı oluşturulmadı."
            );
        }

        if (
            this.connection.state !==
            HubConnectionState.Connected
        ) {
            throw new Error(
                "SignalR bağlantısı aktif değil."
            );
        }
    }

    private ensureConnectionCreated(): void {
        if (!this.connection) {
            throw new Error(
                "Önce callHubService.start() çalıştırılmalıdır."
            );
        }
    }
}

const callHubService =
    new CallHubService();

export default callHubService;