import axios from 'axios';

/* Axios client that is used for authenticated requests
 * every request made with this client get intercepted 
 * and gets the jwt bearer token inserted into its headers
 * every response also gets intercepted in order to do a 
 * in case the response is an error with status 
 * code 401 (unauthorized, because other codes aren't important for this
 * axios instance).
 * If a refresh call has already been made, but more requests come in,
 * they get queued and executed as soon as the new token arrives from the server
 * inspajrd (iskopirao sam ga na max) baj becke <3
 */

const authClient = axios.create({
    headers: {
        'Content-Type': 'application/json',
    },
    baseURL: "http://localhost:1738/api",
})

authClient.interceptors.request.use(
    config => {
        const token = localStorage.getItem("token");
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
)

let refreshing = false;
let failedRequests: any[] = [];

const processRequests = (error: any, token: string | null = null) => {
    failedRequests.forEach(promise => {
        if (error)
            return promise.reject(error);
        else
            return promise.resolve(token);
    });
    failedRequests = [];
}

authClient.interceptors.response.use(
    response => response,
    async (error) => {
        const req = error.config;

        if (req.url?.includes('/RefreshAccess'))
            return Promise.reject(error);
        
        if (error.response?.status === 401) {
            if (refreshing) {
                console.log("drugi if")

                return new Promise((resolve, reject) => {
                    failedRequests.push({resolve, reject})
                }).then(token => {
                    req.headers.Authorization = `Bearer ${token}`;
                    return authClient(req);
                }).catch(error => {
                    return Promise.reject(error);
                });
            }

            refreshing = true;

            const refTok = localStorage.getItem("refreshToken");
            if (refTok) {
                try {
                    const res = await authClient.post("/User/RefreshAccess", {
                        refreshToken: refTok
                    });

                    const newTok = res.data.token;
                    const newRef = res.data.refreshToken;

                    localStorage.setItem("token", newTok);
                    localStorage.setItem("refreshToken", newRef);

                    req.headers.Authorization = `Bearer ${newTok}`;
                    
                    processRequests(null, newTok);
                    
                    return authClient(req);
                    
                } catch (e) {
                    /* prompts the user to login again if refresh token expired */
                    processRequests(e, null);
                    localStorage.removeItem("token");
                    localStorage.removeItem("refreshToken");
                    window.location.href = "/login";
                    return Promise.reject(e);
                } finally {
                    refreshing = false;
                }
            }
            return Promise.reject(error);
        }
    }
)
export default authClient;