import {useState} from "react";
import type Product from "../models/Product.ts";
import {normalizePrice, validatePrice} from "../helpers/price.ts";

export const useProductDraft = (product: Product) => {
    const [draft, setDraft] = useState<Product>(() => ({...product}));

    const setField = <K extends keyof Product>(key: K, value: Product[K]) =>
        setDraft(d => ({...d, [key]: value}));

    const priceError = () => validatePrice(draft.price);

    const toPayload = (): Product => ({
        ...draft,
        price: normalizePrice(draft.price),
    });

    return {draft, setField, toPayload, priceError};
};
