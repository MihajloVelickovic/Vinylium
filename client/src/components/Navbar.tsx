import {Link, useLocation} from "react-router-dom";
import {useState} from "react";
import Cart from "./Cart";



export const Navbar = () => {
    const location = useLocation();
    const isTransparent = location.pathname.startsWith("/products/");

    const [homeFocus, setHomeFocus] = useState(false);
    const [aboutFocus, setAboutFocus] = useState(false);
    const [contactFocus, setContactFocus] = useState(false);
    const [userFocus, setUserFocus] = useState(false);
    const [adminFocus, setAdminFocus] = useState(false);
    
    return(
        <nav className={`navbar ${isTransparent ? "navbar-transparent" : ""}`}>
            <Link className="navbar-brand" to="/">
                <img className="logo navbar-icon" src="../src/assets/vinylium_logo.svg" alt="logo"/>
            </Link>
            <Link className="nav-link" to="/" id="home"
                  onMouseEnter={() => setHomeFocus(true)}
                  onMouseLeave={() => setHomeFocus(false)}
                  style={{filter: homeFocus ? "contrast(50%)" : ""}}
            >
                <img className="navbar-icon" src="../src/assets/home.svg" alt="Home"/></Link>
    
            <Link className="nav-link" to="/about" id="about"
                  onMouseEnter={() => setAboutFocus(true)}
                  onMouseLeave={() => setAboutFocus(false)}
                  style={{filter: aboutFocus ? "contrast(50%)" : ""}}>
                <img className="navbar-icon" src="../src/assets/about.svg" alt="About"/></Link>
    
            <Link className="nav-link" to="/contact" id="contact"
                  onMouseEnter={() => setContactFocus(true)}
                  onMouseLeave={() => setContactFocus(false)}
                  style={{filter: contactFocus ? "contrast(50%)" : ""}}>
                <img className="navbar-icon" src="../src/assets/contact.svg" alt="Contact"/></Link>
    
            <Link className="nav-link" to="/login" id="user"
                  onMouseEnter={() => setUserFocus(true)}
                  onMouseLeave={() => setUserFocus(false)}
                  style={{filter: userFocus ? "contrast(50%)" : ""}}>
                <img className="navbar-icon" src="../src/assets/the-happy-smiler.svg" alt="Login/Register"/></Link>
    
            <Link className="nav-link" to="/admin" id="admin"
                  onMouseEnter={() => setAdminFocus(true)}
                  onMouseLeave={() => setAdminFocus(false)}
                  style={{filter: adminFocus ? "contrast(50%)" : ""}}>
                <img className="admin-dash-img" src="../src/assets/admin_dash.svg" alt="Admin Dashboard"/></Link>
            <Cart/>
        </nav>
    )
}