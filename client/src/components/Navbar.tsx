import {Link} from "react-router-dom";
import Cart from "./Cart";
import {useAuth} from "./AuthContext.tsx";

export const Navbar = () => {
    
    const {isLoggedIn, admin, username} = useAuth();
    
    return(
        <nav className="navbar">
            <Link className="nav-link" to="/">
                <img className="logo navbar-icon" src="../src/assets/vinylium_logo.svg" alt="logo"/>
                <span>Home</span>
            </Link>
            
            <Link className="nav-link" to={isLoggedIn() ? `/user/${username}` : "/login" } id="user">
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
            {admin ?
                <Link className="nav-link" to="/admin" id="admin">
                    <img className="admin-dash-img" src="../src/assets/admin_dash.svg" alt="Admin Dashboard"/>
                    <span>Dashboard</span>
                </Link>
                : <></>}
            <Cart/>
        </nav>
    )
}