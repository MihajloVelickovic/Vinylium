import {createContext, useContext, useEffect, useState} from "react";
import client from "../api/Client.ts";
import Cart from "../models/Cart.ts";

type CartContextData = {
    cart: Cart | null;
    addItem: (barcode: string, storeId: string, quantity?: number) => Promise<void>;
    updateQuantity: (barcode: string, storeId: string, quantity: number) => Promise<void>;
    removeItem: (barcode: string, storeId: string) => Promise<void>;
    clearCart: () => void;
    refreshCart: () => Promise<void>;
    error: string | null;
}

const CartContext = createContext<CartContextData>({} as CartContextData);

export const CartProvider = ({children}) => {
    const [cart, setCart] = useState<Cart | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const fetchCart = async (cartId: string) => {
        await client.get(`/Cart/${cartId}`)
            .then(res => setCart(new Cart(res.data.data)))
            .catch(() => {
                localStorage.removeItem("cartId");
                setCart(null);
            });
    }

    useEffect(() => {
        const cartId = localStorage.getItem("cartId");
        if (!cartId) {
            setLoading(false);
            return;
        }

        fetchCart(cartId).finally(() => setLoading(false));
    }, []);
    
    const refreshCart = async () => {
        if (!cart) 
            return;
        await fetchCart(cart.id);
    }

    const persist = (updated: Cart) => {
        setCart(updated);
        localStorage.setItem("cartId", updated.id);
    }
    
    const applyCartResponse = (data: any) => {
        if (data === null)
            clearCart();
        else
            persist(new Cart(data));
    }

    const addItem = async (barcode: string, storeId: string, quantity: number = 1) => {
        await client.post("/Cart/AddItem", {
            cartId: cart?.id ?? null,
            storeId,
            barcode,
            quantity
        }).then(res => persist(new Cart(res.data.data)))
          .catch(e => setError(e.response?.data ?? e.message));
    }

    const updateQuantity = async (barcode: string, storeId: string, quantity: number) => {
        if (!cart) return;
        await client.put("/Cart/UpdateItem", {
            cartId: cart.id,
            storeId,
            barcode,
            quantity
        }).then(res => applyCartResponse(res.data.data))
          .catch(e => setError(e.response?.data ?? e.message));
    }

    const removeItem = async (barcode: string, storeId: string) => {
        if (!cart) return;
        await client.delete(`/Cart/RemoveItem/${cart.id}/${storeId}/${barcode}`)
            .then(res => applyCartResponse(res.data.data))
            .catch(e => setError(e.response?.data ?? e.message));
    }

    const clearCart = () => {
        setCart(null);
        localStorage.removeItem("cartId");
    }

    return (
        <CartContext value={{cart, addItem, updateQuantity, removeItem, clearCart, refreshCart, error}}>
            {loading ? null : children}
        </CartContext>
    )
}

export const useCart = () => useContext(CartContext);
