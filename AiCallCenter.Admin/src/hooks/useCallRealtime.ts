import {
    useCallback,
    useEffect,
    useRef,
    useState
} from "react";

import callHubService from "../services/callHubService";

import type {
    RealtimeCallMessage,
    RealtimeCallStatus
} from "../services/callHubService";

interface UseCallRealtimeParameters {
    companyId: string | null;
    callId: string | null;
    onMessageCreated?: (
        message: RealtimeCallMessage
    ) => void;
    onStatusChanged?: (
        status: RealtimeCallStatus
    ) => void;
}

interface UseCallRealtimeResult {
    isConnected: boolean;
    errorMessage: string | null;
}

export function useCallRealtime({
    companyId,
    callId,
    onMessageCreated,
    onStatusChanged
}: UseCallRealtimeParameters): UseCallRealtimeResult {
    const [isConnected, setIsConnected] =
        useState(false);

    const [errorMessage, setErrorMessage] =
        useState<string | null>(null);

    const onMessageCreatedRef =
        useRef(onMessageCreated);

    const onStatusChangedRef =
        useRef(onStatusChanged);

    useEffect(() => {
        onMessageCreatedRef.current =
            onMessageCreated;
    }, [onMessageCreated]);

    useEffect(() => {
        onStatusChangedRef.current =
            onStatusChanged;
    }, [onStatusChanged]);

    const joinCurrentGroup =
        useCallback(async (): Promise<void> => {
            if (callId) {
                await callHubService.joinCall(callId);
                return;
            }

            if (companyId) {
                await callHubService.joinCompany(
                    companyId
                );
            }
        }, [companyId, callId]);

    useEffect(() => {
        let cancelled = false;

        const messageHandler = (
            message: RealtimeCallMessage
        ): void => {
            if (
                callId &&
                message.callId !== callId
            ) {
                return;
            }

            onMessageCreatedRef.current?.(
                message
            );
        };

        const statusHandler = (
            status: RealtimeCallStatus
        ): void => {
            if (
                callId &&
                status.callId !== callId
            ) {
                return;
            }

            onStatusChangedRef.current?.(
                status
            );
        };

        const initialize =
            async (): Promise<void> => {
                try {
                    await callHubService.start();

                    if (cancelled) {
                        return;
                    }

                    callHubService
                        .onMessageCreated(
                            messageHandler
                        );

                    callHubService
                        .onCallStatusChanged(
                            statusHandler
                        );

                    callHubService
                        .onReconnected(
                            joinCurrentGroup
                        );

                    await joinCurrentGroup();

                    if (!cancelled) {
                        setErrorMessage(null);
                        setIsConnected(true);
                    }
                } catch (error) {
                    console.error(
                        "SignalR başlatma hatası:",
                        error
                    );

                    if (!cancelled) {
                        setIsConnected(false);

                        setErrorMessage(
                            error instanceof Error
                                ? error.message
                                : "SignalR bağlantısı kurulamadı."
                        );
                    }
                }
            };

        void initialize();

        return () => {
            cancelled = true;

            callHubService
                .offMessageCreated(
                    messageHandler
                );

            callHubService
                .offCallStatusChanged(
                    statusHandler
                );

            if (
                callId &&
                callHubService.isConnected
            ) {
                void callHubService.leaveCall(
                    callId
                );
            } else if (
                companyId &&
                callHubService.isConnected
            ) {
                void callHubService.leaveCompany(
                    companyId
                );
            }
        };
    }, [
        companyId,
        callId,
        joinCurrentGroup
    ]);

    return {
        isConnected,
        errorMessage
    };
}
