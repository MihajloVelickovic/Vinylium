import { FetchAlbumsForm } from './components/FetchAlbumsForm';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import "../src/index.css"

function Home() {
    return <h1>Home Page</h1>;
}

function About() {
    return <h1>About Page</h1>;
}

function Contact() {
    return <h1>Contact Page</h1>;
}

function AddAlbum() {
   return <h2>Search for an album</h2>
}

function App() {
    
    return (
        <div>
            <BrowserRouter>
                {/* Navigation */}
                <nav className="navbar">
                    <a className="navbar-brand" href="/">
                        <img className="logo" src="../src/assets/vinylium_logo.svg" alt="logo" />
                    </a>
                    <Link className="nav-link" to="/">Home</Link> 
                    <Link className="nav-link" to="/about">About</Link> 
                    <Link className="nav-link" to="/contact">Contact</Link>
                    <Link className="nav-link" to="/add-album">Add Album</Link>
                </nav>
                
                {/* Routes */}
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/about" element={<About />} />
                    <Route path="/contact" element={<Contact />} />
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
