import { useState } from "react";
import * as React from "react";
import axios from "axios";
import "../styles/RegisterForm.css"

export default function RegisterForm() {
    const [isRegister, setIsRegister] = useState(false);
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [success, setSuccess] = useState(false);

    const handleRegister = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        console.log('Submit');
        setError("");

        try {
            const response = await axios.post('http://localhost:1738/api/User/Register', {
                email,
                username,
                password,
            });
            console.log("Registration successful: ", response.data);
        }
        catch (error) {
            console.error("Registration failed", error);
        }
    };

    const handleLogin = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        console.log('Submit');
        setError("");

        try {
            const response = await axios.post('http://localhost:1738/api/User/Login', {
                emailOrUsername: email,
                password,
            });
            console.log("Login successful: ", response.data);
            
            /* saves the jwt in a cookie */
            // const date = new Date();
            // date.setTime(date.getTime() + 10 * 365 * 24 * 60 * 60 * 1000);
            // let expiration = "expires=" + date.toUTCString();
            // document.cookie = "vinylium_session" + '=' + response.data.token + ';' + expiration + ';path=/';
            // document.cookie = "vinylium_refresh" + '=' + response.data.refreshToken + ';' + expiration + ';path=/';
            
        }
        catch (error) {
            console.error("Login failed", error);
        }
    };
    
    return (
        <div className="registrationForm">
            <div>
            <button onClick={() => {
                setIsRegister(false); 
                setEmail("");
                setUsername("");
                setPassword("");
            }} 
                    className="button-main loginRegButton"
                    style={{backgroundColor: isRegister ? "" : "var(--vinylium-accent)"}}>Login</button>
            <button onClick={() => {{
                setIsRegister(true);
                setEmail("");
                setUsername("");
                setPassword("");
            }}} 
                    className="button-main loginRegButton"
                    style={{backgroundColor: !isRegister ? "" : "var(--vinylium-accent)"}}>Register</button>
            </div>
            <form onSubmit={isRegister ? handleRegister : handleLogin}>
            <div>
                <input
                    type="email"
                    placeholder={isRegister ? "Email" : "Email or Username"}
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                />
            </div>
            {isRegister ? <div>
                <input
                    type="text"
                    placeholder="Username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                />
            </div> : null}
            <div>
                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
            </div>
            <button type="submit" className="button-main submitButton">{isRegister ? "Register" : "Login"}</button>
        </form>
        </div>
    );

}