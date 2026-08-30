import {useEffect} from "react";
import {useNavigate} from "react-router-dom";
import {useCart} from "./CartContext.tsx";
import "../styles/CartPage.css"

export const CartPage = () => {

    const {cart, updateQuantity, removeItem, refreshCart} = useCart();
    const navigate = useNavigate();
    
    useEffect(() => {
        refreshCart();
    }, []);

    const items = cart?.items ?? [];

    return (
        <div className="cart-page">
            <h1>Cart</h1>
            {
                items.length === 0 ?
                    <p>Your cart is empty.</p> :
                    <>
                        <div className="cart-items">
                            {items.map(item => (
                                <div className="cart-item" key={item.barcode + item.storeId}>
                                    <img src={item.imageUrl} alt={item.barcode + " cover"} width={80} height={80}/>
                                    <div className="cart-item-info">
                                        <p className="cart-item-title">{item.artist} - {item.name}</p>
                                        <p className="cart-item-store">{item.storeName}</p>
                                        <p>{item.price} RSD</p>
                                    </div>
                                    <input type="number" min={1} value={item.quantity}
                                           onChange={e => updateQuantity(item.barcode, item.storeId, Number(e.target.value))}/>
                                    <p className="cart-item-subtotal">{(item.price ?? 0) * item.quantity} RSD</p>
                                    <button className="button-main" type="button"
                                            onClick={() => removeItem(item.barcode, item.storeId)}>Remove</button>
                                </div>
                            ))}
                        </div>
                        <div className="cart-total">
                            <p>Total: {cart?.total() ?? 0} RSD</p>
                            <button className="button-main" type="button"
                                    onClick={() => navigate("/checkout")}>Checkout</button>
                        </div>
                    </>
            }
        </div>
    )
}
