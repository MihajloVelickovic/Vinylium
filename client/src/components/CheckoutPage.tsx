import {useEffect, useState} from "react";
import {useCart} from "./CartContext.tsx";
import {useAuth} from "./AuthContext.tsx";
import authClient from "../api/AuthClient.ts";
import Order from "../models/Order.ts";
import "../styles/CheckoutPage.css"

export const CheckoutPage = () => {

    const {cart, clearCart, refreshCart} = useCart();
    const {token} = useAuth();
    const [email, setEmail] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [confirmedOrder, setConfirmedOrder] = useState<Order | null>(null);

    const items = cart?.items ?? [];
    
    useEffect(() => {
        refreshCart();
    }, []);
    
    useEffect(() => {
        if (!token) 
            return;
        try{
            const payload = JSON.parse(atob(token.split(".")[1]));
            if (payload.email)
                setEmail(prev => prev || payload.email);
        } 
        catch(e){
            console.error(e);
        }
    }, [token]);

    const finish = async () => {
        if (!cart) 
            return;
        
        setSubmitting(true);
        setError(null);

        await authClient.post("/Order/Checkout", {
            cartId: cart.id,
            email
        }).then(res => {
            setConfirmedOrder(new Order(res.data.order));
            clearCart();
        }).catch(e => setError(e.response?.data?.message ?? e.message))
          .finally(() => setSubmitting(false));
    }

    if (confirmedOrder) {
        return (
            <div className="checkout-page">
                <h1>Order Placed</h1>
                <p>Thank you! A confirmation has been recorded for {confirmedOrder.email}.</p>
                <p>Order #{confirmedOrder.id}</p>
                <div className="checkout-items">
                    {confirmedOrder.items.map(item => (
                        <div className="checkout-item" key={item.productBarcode + item.storeId}>
                            <p>{item.productBarcode}</p>
                            <p>x{item.quantity}</p>
                            <p>{item.unitPrice} RSD</p>
                        </div>
                    ))}
                </div>
                <p className="checkout-total">Total: {confirmedOrder.total()} RSD</p>
            </div>
        )
    }

    return (
        <div className="checkout-page">
            <h1>Checkout</h1>
            {
                items.length === 0 ?
                    <p>Your cart is empty.</p> :
                    <>
                        <div className="checkout-items">
                            {items.map(item => (
                                <div className="checkout-item" key={item.barcode + item.storeId}>
                                    <img src={item.imageUrl} alt={item.barcode + " cover"} width={64} height={64}/>
                                    <div className="checkout-item-details">
                                        <p>{item.artist} - {item.name}</p>
                                        <p className="checkout-item-store">{item.storeName}</p>
                                    </div>
                                    <p>x{item.quantity}</p>
                                    <p>{(item.price ?? 0) * item.quantity} RSD</p>
                                </div>
                            ))}
                        </div>
                        <p className="checkout-total">Total: {cart?.total() ?? 0} RSD</p>

                        <input type="email" placeholder="Email" required value={email}
                               onChange={e => setEmail(e.target.value)}/>

                        {error && <p className="error">{error}</p>}

                        <button className="button-main" type="button" disabled={submitting || !email}
                                onClick={finish}>Finish</button>
                    </>
            }
        </div>
    )
}
