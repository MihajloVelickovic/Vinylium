import {createContext, type Dispatch, type SetStateAction, useContext, useEffect, useState} from "react";
import User from "../models/User.ts";
import {useNavigate} from "react-router-dom";
import axios from "axios";

type AuthContextData = {
    user: User | null;
    token: string | null;
    refreshToken: string | null;
    register: (email: string, username: string, password: string) => void;
    login: (emailOrUsername: string, password: string) => void;
    logout: (refTok: string | null) => void;
    isLoggedIn: () => boolean;
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
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [message, setMessage] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    
    useEffect(() => {
        const token = localStorage.getItem("token");
        const refreshToken = localStorage.getItem("refreshToken");
        if (token && refreshToken) {
            setToken(token);
            setRefreshToken(refreshToken);
        }
        setLoading(false);
    }, [token])

    const register = async (email: string, username: string, password: string) => {
        await axios.post("http://localhost:1738/api/User/Register", {
            email: email,
            username: username,
            password: password
        }).then(result => {
            
            localStorage.setItem("token", JSON.stringify(result.data.token));
            localStorage.setItem("refreshToken", JSON.stringify(result.data.refreshToken));
            setToken(result.data.token);
            setRefreshToken(result.data.refreshToken);
            setMessage("Successful registration"); //TODO Write server side messages
            setTimeout(() => {setMessage(null); navigate("/")}, 1000);
        }).catch(e => setError(e.response?.data ?? e.message));
        
    }
    
    const login = async (emailOrUsername: string, password: string) => {
   
        await axios.post("http://localhost:1738/api/User/Login", {
            emailOrUsername: emailOrUsername,
            password: password
        }).then(result => {
            localStorage.setItem("token", JSON.stringify(result.data.token));
            localStorage.setItem("refreshToken", JSON.stringify(result.data.refreshToken));
            setToken(result.data.token);
            setRefreshToken(result.data.refreshToken);
            setMessage("Successful login");
            setTimeout(() => navigate("/"), 1000);
        }).catch(e => {
            setError(e.response?.data ?? e.message)
        });

    }
    
    const logout = async (refTok) => {
        
        await axios.post("http://localhost:1738/api/User/Logout", {
            refTok
        }).then(_ => {})
            .catch(e => setError(e.response?.data ?? e.message));
    }
    
    const isLoggedIn = () => {
        return token != null;
    }
    
    return (
        <AuthContext value={{user, token, refreshToken, register, login, logout, isLoggedIn, message, setMessage, error, setError}}>
            {loading ? null : children}
        </AuthContext>
    )
}

/* hook */
export const useAuth = () => useContext(AuthContext);
