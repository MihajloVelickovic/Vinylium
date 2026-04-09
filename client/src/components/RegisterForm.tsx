import {useState} from "react";
import * as React from "react";
import axios from "axios";
import "../styles/RegisterForm.css"
import {useNavigate} from "react-router-dom";

export default function RegisterForm() {
    const [isRegister, setIsRegister] = useState(false);
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [success, setSuccess] = useState(false);
    
    const navigate = useNavigate();
    
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
            console.log("Registration successful: ");
            navigate("/login");
            
            /* kinda stupid but why not */
            setError("Login successful");
            
        } catch (error: any) {
            if(error.response && error.response.data)
                setError(error.response.data);
            console.error("Registration failed", error);
        }
    };

    const handleLogin = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        console.log('Submit');
        setError("");

        try {
            setEmail(email.replace(/\s/g, ''));
            const response = await axios.post('http://localhost:1738/api/User/Login', {
                emailOrUsername: email,
                password,
            });
            console.log("Login successful: ");
            localStorage.setItem("token", response.data.token);
            localStorage.setItem("refreshToken", response.data.refreshToken);
            localStorage.setItem("user", JSON.stringify(response.data.user));
            navigate("/")
            
        } catch (error:any) {
            if(error.response && error.response.data)
                setError(error.response.data);
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
                    setError("");
                }}
                        className="button-main loginRegButton"
                        style={{backgroundColor: isRegister ? "" : "var(--vinylium-accent)"}}>Login
                </button>
                <button onClick={() => {
                    {
                        setIsRegister(true);
                        setEmail("");
                        setUsername("");
                        setPassword("");
                        setError("");
                    }
                }}
                        className="button-main loginRegButton"
                        style={{backgroundColor: !isRegister ? "" : "var(--vinylium-accent)"}}>Register
                </button>
            </div>
            <form onSubmit={isRegister ? handleRegister : handleLogin}>
                <div>
                    <input
                        type={isRegister ? "email" : "text"}
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
                <h1 style={{color: "indianred"}}>{error}</h1>
            </form>
        </div>
    );

}