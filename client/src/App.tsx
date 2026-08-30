import {FetchAlbumsForm} from './components/FetchAlbumsForm';
import {BrowserRouter, Routes, Route} from 'react-router-dom';
import "../src/index.css"
import RegisterForm from "./components/RegisterForm";
import Store from "./components/Store";
import {ProductPage} from "./components/ProductPage";
import {AdminDashboard} from "./components/AdminDashboard";
import {Navbar} from "./components/Navbar";
import {AuthProvider} from "./components/AuthContext";
import {CartProvider} from "./components/CartContext";
import {CartPage} from "./components/CartPage.tsx";
import {CheckoutPage} from "./components/CheckoutPage.tsx";
import {AdminRoute} from "./components/AdminRoute";
import {PrivateRoute} from "./components/PrivateRoute";
import {Profile} from "./components/Profile";
import {AddStoreForm} from "./components/AddStoreForm.tsx";
import {ManageProductsPage} from "./components/ManageProductsPage.tsx";
import {ManageStoresPage} from "./components/ManageStoresPage.tsx";
import {ManageUsersPage} from "./components/ManageUsersPage.tsx";
import {ContactPage} from "./components/ContactPage.tsx";
import {EditProductPage} from "./components/EditProductPage.tsx";
import {EditStorePage} from "./components/EditStorePage.tsx";

function About() {
    return <h2>About Page</h2>;
}

function App() {
    
    return (
            
            <BrowserRouter>
                <AuthProvider>
                <CartProvider>
                    {/* Navigation */}
                    <Navbar/>
                    {/* Routes */}
                    <Routes>
                        <Route path="/" element={
                            <Store/>
                        }/>
                        <Route path="/user/:username" element={
                            <PrivateRoute> 
                                <Profile/>
                            </PrivateRoute>
                        }/>
                        <Route path="/about" element={<About/>}/>
                        <Route path="/contact" element={<ContactPage/>}/>
                        <Route path="/products/:id" element={<ProductPage/>}/>
                        <Route path="/login" element={
                            <>
                                <RegisterForm/>
                            </>
                        }/>
                        <Route path="/admin" element={
                            <AdminRoute>   
                                <AdminDashboard/>
                            </AdminRoute>
                        }>
                            <Route path="add-album" element={<FetchAlbumsForm/>}/>
                            <Route path="add-store" element={<AddStoreForm/>}/>
                            <Route path="manage-products" element={<ManageProductsPage/>}/>
                            <Route path="manage-products/:id" element={<EditProductPage/>}/>
                            <Route path="manage-stores" element={<ManageStoresPage/>}/>
                            <Route path="manage-stores/:id" element={<EditStorePage/>}/>
                            <Route path="manage-users" element={<ManageUsersPage/>}/>
                            
                        </Route>
                        <Route path="/cart" element={<CartPage/>}/>
                        <Route path="/checkout" element={<CheckoutPage/>}/>

                    </Routes>
                </CartProvider>
                </AuthProvider>
            </BrowserRouter>
                
            
    );
}

export default App
