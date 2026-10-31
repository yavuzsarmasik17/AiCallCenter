import axios from "axios";

const apiClient = axios.create({
    baseURL:
        import.meta.env.VITE_API_BASE_URL ??
        "",
    headers: {
        "Content-Type": "application/json"
    }
});

apiClient.interceptors.request.use(
    (config) => {
        const token =
            localStorage.getItem(
                "accessToken"
            );

        if (token) {
            config.headers.Authorization =
                `Bearer ${token}`;
        }

        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (
            error.response?.status === 401
        ) {
            localStorage.removeItem(
                "accessToken"
            );
            localStorage.removeItem(
                "refreshToken"
            );
            localStorage.removeItem(
                "user"
            );
            localStorage.removeItem(
                "companyId"
            );

            if (
                window.location.pathname !==
                "/login"
            ) {
                window.location.href =
                    "/login";
            }
        }

        return Promise.reject(error);
    }
);

export default apiClient;