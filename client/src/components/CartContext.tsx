import {createContext, useContext, useEffect, useState} from "react";
import authClient from "../api/AuthClient.ts";
import Cart from "../models/Cart.ts";
import {useToast} from "./ToastContext.tsx";
import {useAuth} from "./AuthContext.tsx";
import {apiError} from "../helpers/apiError.ts";

type CartContextData = {
    cart: Cart | null;
    addItem: (barcode: string, storeId: string, quantity?: number) => Promise<void>;
    updateQuantity: (barcode: string, storeId: string, quantity: number) => Promise<void>;
    removeItem: (barcode: string, storeId: string) => Promise<void>;
    clearCart: () => void;
    refreshCart: () => Promise<void>;
}

type MergeToast = {
    text: string;
    kind: "success" | "info";
}

const CartContext = createContext<CartContextData>({} as CartContextData);

const mergeToast = (result: any): MergeToast | null => {
    const merged: number = result?.mergedItems ?? 0;
    const capped: string[] = result?.capped ?? [];
    const dropped: string[] = result?.dropped ?? [];

    if (merged === 0 && capped.length === 0 && dropped.length === 0)
        return null;

    const parts: string[] = [];

    if (merged > 0)
        parts.push(`Merged ${merged} ${merged === 1 ? "item" : "items"} from your guest cart`);

    if (capped.length > 0)
        parts.push(`limited to available stock: ${capped.join(", ")}`);

    if (dropped.length > 0)
        parts.push(`out of stock and removed: ${dropped.join(", ")}`);

    return {
        text: parts.join(" — "),
        kind: capped.length > 0 || dropped.length > 0 ? "info" : "success"
    };
};

export const CartProvider = ({children}) => {
    const [cart, setCart] = useState<Cart | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const {notify} = useToast();
    const {token} = useAuth();

    const fetchGuestCart = async (cartId: string) => {
        await authClient.get(`/Cart/${cartId}`)
            .then(res => setCart(new Cart(res.data.data)))
            .catch(() => {
                localStorage.removeItem("cartId");
                setCart(null);
            });
    }

    const fetchMyCart = async () => {
        await authClient.get("/Cart/Mine")
            .then(res => setCart(res.data.data ? new Cart(res.data.data) : null))
            .catch(() => setCart(null));
    }

    const mergeGuestCart = async (guestCartId: string) => {
        await authClient.post("/Cart/Merge", {guestCartId})
            .then(res => {
                const result = res.data.data;
                localStorage.removeItem("cartId");
                setCart(result?.cart ? new Cart(result.cart) : null);

                const toast = mergeToast(result);
                if (toast)
                    notify(toast.text, toast.kind);
            })
            .catch(async e => {
                notify(apiError(e, "Could not merge your guest cart"), "error");
                await fetchMyCart();
            });
    }

    useEffect(() => {
        const sync = async () => {
            const guestCartId = localStorage.getItem("cartId");

            if (!token) {
                if (guestCartId)
                    await fetchGuestCart(guestCartId);
                else
                    setCart(null);
                return;
            }

            if (guestCartId)
                await mergeGuestCart(guestCartId);
            else
                await fetchMyCart();
        }

        sync().finally(() => setLoading(false));
    }, [token]);

    const refreshCart = async () => {
        if (token) {
            await fetchMyCart();
            return;
        }

        if (!cart)
            return;

        await fetchGuestCart(cart.id);
    }

    const persist = (updated: Cart) => {
        setCart(updated);
        if (!token)
            localStorage.setItem("cartId", updated.id);
    }

    const applyCartResponse = (data: any) => {
        if (data === null)
            clearCart();
        else
            persist(new Cart(data));
    }

    const addItem = async (barcode: string, storeId: string, quantity: number = 1) => {
        await authClient.post("/Cart/AddItem", {
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
        await authClient.put("/Cart/UpdateItem", {
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

        await authClient.delete(`/Cart/RemoveItem/${cart.id}/${storeId}/${barcode}`)
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
