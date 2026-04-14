import axios from 'axios';

const authClient = axios.create({
    headers: {
        'Content-Type': 'application/json',
    },
    baseURL: "http://localhost:1738/api",
})

authClient.interceptors.request.use(
    config => {
        const token = localStorage.getItem("token");
        if(token) {
            config.headers.Authorization = `Bearer ${token.replaceAll("\"","")}`;
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
)

authClient.interceptors.response.use(
    response => response,
    async (error) => {
        const req = error.config;
        
        if(req.url?.includes('/RefreshAccess'))
            return Promise.reject(error);
        
        const refTok = localStorage.getItem("refreshToken");
        
        if(refTok) {
            try {
                const res = await authClient.post("/User/RefreshAccess", {
                    refreshToken: refTok
                });
    
                const newTok = res.data.token;
                const newRef = res.data.refreshToken;
    
                localStorage.setItem("token", newTok);
                localStorage.setItem("refreshToken", newRef);
                
                req.headers.Authorization = `Bearer ${newTok}`;
                return authClient(req)
                
            }
            catch (e) {
                console.error(e);
            }
        }
    }
)

export default authClient;
    
