import { useState } from "react";
import type Store from "../models/Store.ts";

const toHourMinute = (time: string) => {
    if (!time) return "";
    const hours = time.split(":");
    return hours.length < 2 ? time : `${hours[0]}:${hours[1]}`;
};

export const useStoreDraft = (store: Store) => {
    const [draft, setDraft] = useState<Store>(() => ({...store,
        openingHours: toHourMinute(store.openingHours),
        closingHours: toHourMinute(store.closingHours)}));
    
    const setField  = <K extends keyof Store>(key: K, value: Store[K]) => 
        setDraft(d => ({...d, [key]: value}));
    
    const toPayload = () => ({
        id: draft.id,
        name: draft.name.trim(),
        address: draft.address.trim(),
        city: draft.city.trim(),
        contactNumber: draft.contactNumber.trim(),
        openingHours: toHourMinute(draft.openingHours),
        closingHours: toHourMinute(draft.closingHours)
    });
    return {draft, setField, toPayload};
}