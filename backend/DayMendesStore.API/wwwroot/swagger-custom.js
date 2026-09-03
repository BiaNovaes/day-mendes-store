(function () {
    const TOKEN_KEY = "day_mendes_store_jwt_token";

    function showToast(message, isSuccess = true) {
        let toast = document.getElementById("swagger-auto-auth-toast");
        if (!toast) {
            toast = document.createElement("div");
            toast.id = "swagger-auto-auth-toast";
            toast.style.position = "fixed";
            toast.style.bottom = "24px";
            toast.style.right = "24px";
            toast.style.zIndex = "99999";
            toast.style.padding = "14px 22px";
            toast.style.borderRadius = "8px";
            toast.style.fontFamily = "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif";
            toast.style.fontSize = "14px";
            toast.style.fontWeight = "600";
            toast.style.boxShadow = "0 6px 16px rgba(0,0,0,0.3)";
            toast.style.transition = "opacity 0.3s ease, transform 0.3s ease";
            document.body.appendChild(toast);
        }

        toast.style.backgroundColor = isSuccess ? "#2e7d32" : "#c62828";
        toast.style.color = "#ffffff";
        toast.textContent = message;
        toast.style.opacity = "1";
        toast.style.transform = "translateY(0)";
        toast.style.display = "block";

        setTimeout(() => {
            toast.style.opacity = "0";
            toast.style.transform = "translateY(10px)";
            setTimeout(() => {
                toast.style.display = "none";
            }, 300);
        }, 4500);
    }

    function applyTokenToSwagger(token) {
        if (!token || typeof token !== "string") return;

        let cleanToken = token.trim();
        if (cleanToken.toLowerCase().startsWith("bearer ")) {
            cleanToken = cleanToken.substring(7).trim();
        }

        if (!cleanToken) return;

        if (window.ui) {
            // 1. Suporte para esquema Http Bearer do OpenAPI 3.0
            if (window.ui.authActions && typeof window.ui.authActions.authorize === "function") {
                window.ui.authActions.authorize({
                    Bearer: {
                        name: "Bearer",
                        schema: {
                            type: "http",
                            in: "header",
                            scheme: "bearer",
                            bearerFormat: "JWT"
                        },
                        value: cleanToken
                    }
                });
            }

            // 2. Suporte para esquema ApiKey se presente
            if (typeof window.ui.preauthorizeApiKey === "function") {
                window.ui.preauthorizeApiKey("Bearer", cleanToken);
            }

            console.log("[Day Mendes Store] Token JWT autorizado com sucesso no Swagger!");
        }
    }

    function handleLoginResponse(jsonData) {
        if (!jsonData || typeof jsonData !== "object") return;

        const token = jsonData.token || jsonData.Token || jsonData.data?.token || jsonData.data?.Token;
        if (token) {
            localStorage.setItem(TOKEN_KEY, token);
            applyTokenToSwagger(token);
            showToast("🔒 Login realizado! Swagger autorizado automaticamente.");
        }
    }

    // 1. Interceptar Fetch API
    const originalFetch = window.fetch;
    window.fetch = async function (...args) {
        const response = await originalFetch.apply(this, args);
        try {
            const url = typeof args[0] === "string" ? args[0] : (args[0] && args[0].url ? args[0].url : "");
            if (url && (url.includes("/auth/login") || url.includes("/login"))) {
                const clone = response.clone();
                clone.json().then(data => {
                    handleLoginResponse(data);
                }).catch(() => {});
            }
        } catch (err) {
            console.error("[Day Mendes Store] Erro ao interceptar resposta de login:", err);
        }
        return response;
    };

    // 2. Interceptar XMLHttpRequest
    const originalXhrOpen = XMLHttpRequest.prototype.open;
    const originalXhrSend = XMLHttpRequest.prototype.send;

    XMLHttpRequest.prototype.open = function (method, url, ...rest) {
        this._requestUrl = url;
        return originalXhrOpen.apply(this, [method, url, ...rest]);
    };

    XMLHttpRequest.prototype.send = function (...args) {
        this.addEventListener("load", function () {
            try {
                if (this._requestUrl && (this._requestUrl.includes("/auth/login") || this._requestUrl.includes("/login"))) {
                    const data = JSON.parse(this.responseText);
                    handleLoginResponse(data);
                }
            } catch (e) {}
        });
        return originalXhrSend.apply(this, args);
    };

    // 3. Restaurar autenticação automaticamente ao carregar/recarregar a página
    let attempts = 0;
    const checkInterval = setInterval(() => {
        attempts++;
        if (window.ui) {
            clearInterval(checkInterval);
            const savedToken = localStorage.getItem(TOKEN_KEY);
            if (savedToken) {
                applyTokenToSwagger(savedToken);
                console.log("[Day Mendes Store] Token JWT pré-carregado no Swagger a partir da sessão anterior.");
            }
        }
        if (attempts > 50) {
            clearInterval(checkInterval);
        }
    }, 100);
})();
