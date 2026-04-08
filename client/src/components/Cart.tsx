import { useState } from 'react';


function Cart() {
    const [isOpen, setIsOpen] = useState(false);
    const [cartItems, setCartItems] = useState([]);
    
    return (
        <div className="cart-wrapper">
            <img 
                onClick={() => setIsOpen(!isOpen)}
                className="cart"
                src="../src/assets/cart.svg" 
            />

            {isOpen && (
                <div className="cart-dropdown">
                    {cartItems.length > 0 ? (
                        <ul>
                            {cartItems.map((item, index) => (
                                <li key={index}>{item}</li>
                            ))}
                        </ul>
                    ) : (
                        <p>Cart is empty</p>
                    )}
                </div>
            )}
        </div>
    );
}

export default Cart;