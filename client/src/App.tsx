import {FetchAlbumsForm} from './components/FetchAlbumsForm';
import {BrowserRouter, Routes, Route, Link} from 'react-router-dom';
import "../src/index.css"
import RegisterForm from "./components/RegisterForm";
import {Store} from "./components/Store";
import {useEffect, useState} from "react";
import {ProductPage} from "./components/ProductPage.tsx";
import {AdminDashboard} from "./components/AdminDashboard";
import { Navbar } from "./components/Navbar";

function Home() {
    return <h2>Home Page</h2>;
}

function About() {
    return <h2>About Page</h2>;
}

function Contact() {
    return <h2>Contact Page</h2>;
}



function App() {
    
    const [loggedIn, setLoggedIn] = useState(false);

    useEffect(() => {
        localStorage.getItem("token") === null ? setLoggedIn(false) : setLoggedIn(true); 
    }, []);
    
    return (
        <div>
            <BrowserRouter>
                {/* Navigation */}
                <Navbar />
                {/* Routes */}
                <Routes>
                    <Route path="/" element={
                        <>
                            <Home/>
                            <Store />
                        </>
                    }/>
                    <Route path="/user/:username" element={<></>}/>
                    <Route path="/about" element={<About/>}/>
                    <Route path="/contact" element={<Contact/>}/>
                    <Route path="/products/:id" element={<ProductPage/>}/>
                    <Route path="/login" element={
                        <>
                            <RegisterForm/>
                        </>
                    }/>
                    <Route path="/admin" element={<AdminDashboard />}>
                        
                        <Route path="add-album" element={<FetchAlbumsForm />} />
                    </Route>
                    <Route path="/cart"></Route>

                </Routes>
            </BrowserRouter>
        </div>
    );
}

export default App
