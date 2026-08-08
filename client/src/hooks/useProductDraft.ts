import {useState} from "react";
import type Product from "../models/Product.ts";

export const useProductDraft = (product: Product) => {
    const [draft, setDraft] = useState<Product>(() => ({...product}));

    const setField = <K extends keyof Product>(key: K, value: Product[K]) =>
        setDraft(d => ({...d, [key]: value}));
    
    const toPayload = (): Product => ({
        ...draft,
        price: draft.price === null || draft.price.trim() === ""
            ? null
            : draft.price.trim().replace(",", "."),
    });

    return {draft, setField, toPayload};
};
