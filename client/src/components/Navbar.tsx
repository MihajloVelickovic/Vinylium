import {Link, useLocation} from "react-router-dom";
import {useState} from "react";
import Cart from "./Cart";



export const Navbar = () => {
    const location = useLocation();
    const isTransparent = location.pathname.startsWith("/products/");
    
    return(
        <nav className={`navbar ${isTransparent ? "navbar-transparent" : ""}`}>
            <Link className="nav-link" to="/">
                <img className="logo navbar-icon" src="../src/assets/vinylium_logo.svg" alt="logo"/>
                <span>Home</span>
            </Link>
            
            <Link className="nav-link" to="/login" id="user">
                <img className="navbar-icon" src="../src/assets/the-happy-smiler.svg" alt="Login/Register"/>
                <span>User</span>
            </Link>
    
            <Link className="nav-link" to="/about" id="about">
                <img className="navbar-icon" src="../src/assets/about.svg" alt="About"/>
                <span>About</span>
            </Link>
    
            <Link className="nav-link" to="/contact" id="contact">
                <img className="navbar-icon" src="../src/assets/contact.svg" alt="Contact"/>
                <span>Contact</span>
            </Link>
    
            
    
            <Link className="nav-link" to="/admin" id="admin">
                <img className="admin-dash-img" src="../src/assets/admin_dash.svg" alt="Admin Dashboard"/>
                <span>Dashboard</span>
            </Link>
            <Cart/>
        </nav>
    )
}