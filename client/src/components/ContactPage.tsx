import {useEffect, useState} from "react";
import Store from "../models/Store.ts";
import authClient from "../api/AuthClient.ts";
import {StoreCard} from "./StoreCard.tsx";

export const ContactPage = () => {

    const [stores, setStores] = useState<Array<Store>>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const getStores = async () => {
            const pairs = new Array<Store>();
            try {
                const r = await authClient.get("/Store/GetStores")
                r.data.data.forEach((store: any) => {
                    const item = new Store(store);
                    pairs.push(item);
                })
            }
            catch(e){
                console.error(e);
            }
            return pairs;
        }

        getStores().then(p => {
            console.log(p);
            setStores(p);
            setLoading(false);
        }).catch(e => console.error(e));

    }, [])

    return (
        <div className="stores">
            {   !loading &&
                stores.map(s => {
                    return <StoreCard store={s}/>
                })
            }
        </div>
    )
}