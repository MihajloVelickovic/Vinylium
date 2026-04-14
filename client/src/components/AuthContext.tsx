import {createContext, type Dispatch, type SetStateAction, useContext, useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import authClient from "../api/AuthClient";
import client from "../api/Client.ts";

type AuthContextData = {
    username: string | null;
    token: string | null;
    refreshToken: string | null;
    register: (email: string, username: string, password: string) => void;
    login: (emailOrUsername: string, password: string) => void;
    logout: () => void;
    isLoggedIn: () => boolean;
    admin: boolean | null;
    message: string | null;
    setMessage: Dispatch<SetStateAction<string|null>>;
    error: string | null;
    setError: Dispatch<SetStateAction<string|null>>;
}

const AuthContext = createContext<AuthContextData>({} as AuthContextData);

export const AuthProvider = ({children}) => {
    const navigate = useNavigate();
    const [token, setToken] = useState<string | null>(null);
    const [refreshToken, setRefreshToken] = useState<string|null>(null);
    const [username, setUsername] = useState<string | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [message, setMessage] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [admin, setAdmin] = useState<boolean | null>(false);
    
    useEffect(() => {
        //const username = localStorage.getItem("username");
        const token = localStorage.getItem("token");
        const refreshToken = localStorage.getItem("refreshToken");
        if (token && refreshToken) {
            //setUsername(username);
            setToken(token);
            setRefreshToken(refreshToken);
            isAdmin(token).then(res => setAdmin(res));
            getUsername().then(res => {
                setUsername(res.data.username)}
            )
        }
        setLoading(false);
    }, [token, refreshToken]);

    const isAdmin = async (token: string) =>{
        try {
            const res = await client.get("/User/IsAdmin", {
                params: {
                    token
                }
            });
            return res.data.isAdmin;
        }
        catch(e){
            console.error(e);
        }
        return false;
    }
    
    const getUsername = async () => {
        return await authClient.get("/User/GetUsername")
    }
    
    const register = async (email: string, username: string, password: string) => {
        await client.post("/User/Register", {
            email: email,
            username: username,
            password: password
        }).
        then(result => {
            localStorage.setItem("token", result.data.token);
            localStorage.setItem("refreshToken", result.data.refreshToken);
            setToken(result.data.token);
            setRefreshToken(result.data.refreshToken);
            setMessage("Successful registration"); //TODO Write server side messages
            setTimeout(() => {
                setMessage(null); 
                navigate("/")
            }, 1000);
        })
        .catch(e => setError(e.response?.data ?? e.message));
    }
    
    const login = async (emailOrUsername: string, password: string) => {
        
        await client.post("/User/Login", {
            emailOrUsername: emailOrUsername,
            password: password
        }).then(result => {
            localStorage.setItem("token", result.data.token);
            localStorage.setItem("refreshToken", result.data.refreshToken);
            setToken(result.data.token);
            setRefreshToken(result.data.refreshToken);
            setMessage("Successful login");
            setTimeout(() => navigate("/"), 1000);
        }).catch(e => {
            setError(e.response?.data ?? e.message)
        });

    }
    
    const logout = async () => {
        await authClient.post("/User/Logout", {
            refreshToken
        }).then(_ => {
            setToken(null);
            setRefreshToken(null);
            setAdmin(false);
            localStorage.removeItem("token");
            localStorage.removeItem("refreshToken");
            setMessage("Logged Out");
            navigate("/")
        })
          .catch(e => setError(e.response?.data ?? e.message));
    }
    
    const isLoggedIn = () => {
        return token !== null;
    }
    
    return (
        <AuthContext value={{username, token, refreshToken, register, 
                            login, logout, isLoggedIn, message, 
                            setMessage, error, setError, admin}}>
            {loading ? null : children}
        </AuthContext>
    )
}

/* hook */
export const useAuth = () => useContext(AuthContext);
