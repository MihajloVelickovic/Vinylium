
import {useEffect, useState} from "react";
import Store from "../models/Store.ts";
import authClient from "../api/AuthClient.ts";
import {ManageStoreCard} from "./ManageStoreCard.tsx";
import "../styles/ManageStoresPage.css"

export const ManageStoresPage = () => {

    const [stores, setStores] = useState<Array<Store>>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const getStores = async () => {
            const pairs = new Array<Store>();
            const r = await authClient.get("/Store/GetStores")
            r.data.data.forEach((store: any) => {
                const item = new Store(store);
                pairs.push(item);
            })
            return pairs;
        }

        getStores().then(p => {
            setStores(p);
            setLoading(false);
        }).catch(e => {
            /* surfaced instead of only logged, so a failing request reads as an
             * error rather than as an empty list of stores
             */
            setError(e.response?.data ?? e.message ?? "Failed to load stores");
            setLoading(false);
        });

    }, [])

    if (loading)
        return <h2 className="manage-stores-message">Loading stores...</h2>

    if (error)
        return <h2 className="manage-stores-message manage-stores-error">{error}</h2>

    if (stores.length === 0)
        return <h2 className="manage-stores-message">No stores yet.</h2>

    return (
        <div className="manage-stores">
            {
                stores.map(s => {
                    return <ManageStoreCard key={s.id} store={s}/>
                })
            }
        </div>
    )
}
