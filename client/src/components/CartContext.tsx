import {createContext, useContext, useEffect, useState} from "react";
import client from "../api/Client.ts";
import Cart from "../models/Cart.ts";
import {useToast} from "./ToastContext.tsx";
import {apiError} from "../helpers/apiError.ts";

type CartContextData = {
    cart: Cart | null;
    addItem: (barcode: string, storeId: string, quantity?: number) => Promise<void>;
    updateQuantity: (barcode: string, storeId: string, quantity: number) => Promise<void>;
    removeItem: (barcode: string, storeId: string) => Promise<void>;
    clearCart: () => void;
    refreshCart: () => Promise<void>;
}

const CartContext = createContext<CartContextData>({} as CartContextData);

export const CartProvider = ({children}) => {
    const [cart, setCart] = useState<Cart | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const {notify} = useToast();

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
        }).then(res => {
            const updated = new Cart(res.data.data);
            persist(updated);

            const added = updated.items.find(i => i.barcode === barcode && i.storeId === storeId);
            notify(added ? `Added ${added.name} to cart` : "Added to cart");
        })
          .catch(e => notify(apiError(e, "Could not add to cart"), "error"));
    }

    const updateQuantity = async (barcode: string, storeId: string, quantity: number) => {
        if (!cart) return;
        await client.put("/Cart/UpdateItem", {
            cartId: cart.id,
            storeId,
            barcode,
            quantity
        }).then(res => applyCartResponse(res.data.data))
          .catch(e => notify(apiError(e, "Could not update the cart"), "error"));
    }

    const removeItem = async (barcode: string, storeId: string) => {
        if (!cart) return;

        const removed = cart.items.find(i => i.barcode === barcode && i.storeId === storeId);

        await client.delete(`/Cart/RemoveItem/${cart.id}/${storeId}/${barcode}`)
            .then(res => {
                applyCartResponse(res.data.data);
                notify(removed ? `Removed ${removed.name} from cart` : "Removed from cart", "info");
            })
            .catch(e => notify(apiError(e, "Could not remove the item"), "error"));
    }

    const clearCart = () => {
        setCart(null);
        localStorage.removeItem("cartId");
    }

    return (
        <CartContext value={{cart, addItem, updateQuantity, removeItem, clearCart, refreshCart}}>
            {loading ? null : children}
        </CartContext>
    )
}

export const useCart = () => useContext(CartContext);
