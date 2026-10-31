import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel
} from "@microsoft/signalr";

import type {
    MessageSpeaker
} from "./callService";

export interface CallRealtimeMessage {
    id: string;
    companyId: string;
    callId: string;
    speaker: MessageSpeaker;
    content: string;
    spokenAt: string;
    sequenceNumber: number;
    confidenceScore: number | null;
    audioFileUrl: string | null;
}

export interface CallStatusChangedNotification {
    companyId: string;
    callId: string;
    status: number;
}

export type CallMessageCreatedHandler =
    (message: CallRealtimeMessage) => void;

export type CallStatusChangedHandler =
    (
        notification: CallStatusChangedNotification
    ) => void;

let connection: HubConnection | null = null;

function getAccessToken(): string {
    return (
        localStorage.getItem("accessToken") ??
        localStorage.getItem("token") ??
        ""
    );
}

function getConnection(): HubConnection {
    if (connection) {
        return connection;
    }

    connection =
        new HubConnectionBuilder()
            .withUrl(
                "https://localhost:7280/hubs/calls",
                {
                    accessTokenFactory:
                        () => getAccessToken()
                }
            )
            .withAutomaticReconnect()
            .configureLogging(
                LogLevel.Information
            )
            .build();

    return connection;
}

export async function startCallRealtime():
    Promise<void> {
    const currentConnection =
        getConnection();

    if (
        currentConnection.state ===
        HubConnectionState.Connected
    ) {
        return;
    }

    if (
        currentConnection.state ===
        HubConnectionState.Connecting ||
        currentConnection.state ===
        HubConnectionState.Reconnecting
    ) {
        return;
    }

    await currentConnection.start();

    console.log(
        "SignalR bağlantısı kuruldu."
    );
}

export async function stopCallRealtime():
    Promise<void> {
    if (!connection) {
        return;
    }

    if (
        connection.state ===
        HubConnectionState.Disconnected
    ) {
        return;
    }

    await connection.stop();
}

export async function joinCompany(
    companyId: string
): Promise<void> {
    const currentConnection =
        getConnection();

    await startCallRealtime();

    await currentConnection.invoke(
        "JoinCompany",
        companyId
    );
}

export async function leaveCompany(
    companyId: string
): Promise<void> {
    if (
        !connection ||
        connection.state !==
        HubConnectionState.Connected
    ) {
        return;
    }

    await connection.invoke(
        "LeaveCompany",
        companyId
    );
}

export async function joinCall(
    callId: string
): Promise<void> {
    const currentConnection =
        getConnection();

    await startCallRealtime();

    await currentConnection.invoke(
        "JoinCall",
        callId
    );
}

export async function leaveCall(
    callId: string
): Promise<void> {
    if (
        !connection ||
        connection.state !==
        HubConnectionState.Connected
    ) {
        return;
    }

    await connection.invoke(
        "LeaveCall",
        callId
    );
}

export function onCallMessageCreated(
    handler: CallMessageCreatedHandler
): () => void {
    const currentConnection =
        getConnection();

    currentConnection.on(
        "CallMessageCreated",
        handler
    );

    return () => {
        currentConnection.off(
            "CallMessageCreated",
            handler
        );
    };
}

export function onCallStatusChanged(
    handler: CallStatusChangedHandler
): () => void {
    const currentConnection =
        getConnection();

    currentConnection.on(
        "CallStatusChanged",
        handler
    );

    return () => {
        currentConnection.off(
            "CallStatusChanged",
            handler
        );
    };
}

export function getCallRealtimeState():
    HubConnectionState {
    return getConnection().state;
}