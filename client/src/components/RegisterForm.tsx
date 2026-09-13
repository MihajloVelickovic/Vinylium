import {useEffect, useState} from "react";
import * as React from "react";
import "../styles/RegisterForm.css"
import {useAuth} from "./AuthContext.tsx";

export default function RegisterForm() {
    const [isRegister, setIsRegister] = useState(false);
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    
    const {login, register, message, setMessage, error, setError} = useAuth();
    
    useEffect(() => {
        setMessage(null);
        setError(null);
    }, [])
    
    const handleRegister = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setEmail(email.replace(/\s/g, ''));
        register(email, username, password);
    };

    const handleLogin = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setEmail(email.replace(/\s/g, ''));
        login(email, password);
    };

    return (
        <div className="registrationForm">
            <div>
                <button data-testid="auth-mode-login"
                        onClick={() => {
                    setIsRegister(false);
                    setEmail("");
                    setUsername("");
                    setPassword("");
                }}
                        className="button-main loginRegButton"
                        style={{backgroundColor: isRegister ? "" : "var(--vinylium-accent)"}}>Login
                </button>
                <button data-testid="auth-mode-register"
                        onClick={() => {
                    {
                        setIsRegister(true);
                        setEmail("");
                        setUsername("");
                        setPassword("");
                    }
                }}
                        className="button-main loginRegButton"
                        style={{backgroundColor: !isRegister ? "" : "var(--vinylium-accent)"}}>Register
                </button>
            </div>
            <form onSubmit={isRegister ? handleRegister : handleLogin}>
                <div>
                    <input
                        data-testid="auth-identifier"
                        type={isRegister ? "email" : "text"}
                        placeholder={isRegister ? "Email" : "Email or Username"}
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </div>
                {isRegister ? <div>
                    <input
                        data-testid="auth-username"
                        type="text"
                        placeholder="Username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />
                </div> : null}
                <div>
                    <input
                        data-testid="auth-password"
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </div>
                <button type="submit" data-testid="auth-submit" className="button-main submitButton">{isRegister ? "Register" : "Login"}</button>
                <h1 data-testid="auth-error" style={{color: "indianred"}}>{error}</h1>          
                <h1 data-testid="auth-message" style={{color: "lightgreen"}}>{message}</h1>

            </form>
        </div>
    );

}