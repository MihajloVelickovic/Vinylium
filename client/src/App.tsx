import { FetchAlbumsForm } from './components/FetchAlbumsForm';
import { BrowserRouter, Routes, Route, Link} from 'react-router-dom';
import "../src/index.css"
import RegisterForm from "./components/RegisterForm";
import {Store} from "./components/Store";
import {useEffect, useState} from "react";

function Home() {
    return <h2>Home Page</h2>;
}

function About() {
    return <h2>About Page</h2>;
}

function Contact() {
    return <h2>Contact Page</h2>;
}

function AddAlbum() {
   return <h2>Search for an album</h2>
}

function App() {

    const [homeFocus, setHomeFocus] = useState(false);
    const [aboutFocus, setAboutFocus] = useState(false);
    const [contactFocus, setContactFocus] = useState(false);
    const [userFocus, setUserFocus] = useState(false);
    
    return (
        <div>
            <BrowserRouter>
                {/* Navigation */}
                <nav className="navbar">
                    <a className="navbar-brand" href="/">
                        <img className="logo" src="../src/assets/vinylium_logo.svg" alt="logo" />
                    </a>
                    <Link className="nav-link" to="/" id="home"
                          onMouseEnter={() => setHomeFocus(true)}
                          onMouseLeave={() => setHomeFocus(false)}
                          style={{filter: homeFocus ? "contrast(50%)" : ""}}
                    >
                        <img src="../src/assets/home.svg" alt="Home" /></Link> 
                    
                    <Link className="nav-link" to="/about" id="about"
                          onMouseEnter={() => setAboutFocus(true)}
                          onMouseLeave={() => setAboutFocus(false)}
                          style={{filter: aboutFocus ? "contrast(50%)" : ""}}>
                        <img src="../src/assets/about.svg" alt="About" /></Link> 
                    
                    <Link className="nav-link" to="/contact" id="contact"
                          onMouseEnter={() => setContactFocus(true)}
                          onMouseLeave={() => setContactFocus(false)}
                          style={{filter: contactFocus ? "contrast(50%)" : ""}}>
                        <img src="../src/assets/contact.svg" alt="Contact" /></Link>
                    
                    <Link className="nav-link" to="/login" id="user"
                          onMouseEnter={() => setUserFocus(true)}
                          onMouseLeave={() => setUserFocus(false)}
                          style={{filter: userFocus ? "contrast(50%)" : ""}}>
                        <img src="../src/assets/the-happy-smiler.svg" alt="Login/Register"/></Link>
                    
                    <Link className="nav-link" to="/add-album">Add Album</Link>
                </nav>
                
                {/* Routes */}
                <Routes>
                    <Route path="/" element={
                    <>
                        <Home />
                        <Store />
                    </>
                    } />
                    <Route path="/about" element={<About />} />
                    <Route path="/contact" element={<Contact />} />
                    <Route path="/login" element={
                        <>
                            <RegisterForm />
                        </>
                    } />
                    <Route path="/add-album" element={
                        <>
                            <AddAlbum />
                            <FetchAlbumsForm />
                        </>
                        } 
                    />
                    
                </Routes>
            </BrowserRouter>
        </div>
    );
}

export default App
