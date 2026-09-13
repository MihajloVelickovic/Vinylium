import {useNavigate, useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import Store from "../models/Store.ts";
import {Field} from "./Field.tsx";
import {useStoreDraft} from "../hooks/useStoreDraft.ts";
import authClient from "../api/AuthClient.ts";
import {useToast} from "./ToastContext.tsx";
import {apiError} from "../helpers/apiError.ts";
import "../styles/AlbumCard.css"
import "../styles/EditProductPage.css"
import "../styles/EditStorePage.css"


const EditStoreForm = ({store}: { store: Store }) => {

    const navigate = useNavigate();
    const {draft, setField, toPayload} = useStoreDraft(store);
    const [error, setError] = useState("");
    const [busy, setBusy] = useState(false);
    const {notify} = useToast();

    const handleCancel = () => {
        navigate("/admin/manage-stores");
    }

    const handleUpdate = async () => {
        if (!window.confirm(`Save your changes to "${store.name}"?`))
            return;

        setError("");
        setBusy(true);
        try {
            await authClient.put("/Store/UpdateStore", toPayload());
            notify(`Updated "${draft.name}"`);
            /* navigate rather than history.back() so the list remounts and
             * refetches instead of replaying the previous history entry
             */
            navigate("/admin/manage-stores");
        }
        catch (e: any) {
            const message = apiError(e, "Failed to update store");
            setError(message);
            notify(message, "error");
        }
        finally {
            setBusy(false);
        }
    }

    const handleDelete = async () => {
        if (!window.confirm(`Delete "${store.name}"? This also clears its stock and cannot be undone.`))
            return;

        setError("");
        setBusy(true);
        try {
            await authClient.delete(`/Store/DeleteStore/${store.id}`);
            notify(`Deleted "${store.name}"`, "info");
            navigate("/admin/manage-stores");
        }
        catch (e: any) {
            const message = apiError(e, "Failed to delete store");
            setError(message);
            notify(message, "error");
        }
        finally {
            setBusy(false);
        }
    }

    return (
        <div className="editStoreCard" data-testid="edit-store-card">
            <h1 className="editStoreTitle">{store.name}</h1>

            <div className="editStoreFields">
                <Field label="Name:" value={draft.name} testId="edit-store-name"
                       onChange={v => setField("name", v)}/>

                <Field label="Address:" value={draft.address} testId="edit-store-address"
                       onChange={v => setField("address", v)}/>

                <Field label="City:" value={draft.city} testId="edit-store-city"
                       onChange={v => setField("city", v)}/>

                <Field label="Contact No.:" value={draft.contactNumber} testId="edit-store-contact"
                       onChange={v => setField("contactNumber", v)}
                       placeholder="+381601234567"/>

                <Field label="Opens:" value={draft.openingHours} testId="edit-store-opens"
                       onChange={v => setField("openingHours", v)}
                       placeholder="HH:MM"/>

                <Field label="Closes:" value={draft.closingHours} testId="edit-store-closes"
                       onChange={v => setField("closingHours", v)}
                       placeholder="HH:MM"/>
                <Field label="Warehouse?:" testId="edit-store-warehouse"
                       contentEditable={false}
                       onClick={v => setField("isWarehouse", !draft.isWarehouse)}
                       onChange={()=>{}}
                       value={draft.isWarehouse ? "Yes" : "No"}/>
            </div>

            {error && <p className="editStoreError" data-testid="edit-store-error">{error}</p>}

            <div className="buttonsEdit">
                <button className="buttonEdit cancelEdit" data-testid="edit-store-cancel" onClick={handleCancel}>Cancel Update</button>
                <button className="buttonEdit updateEdit" data-testid="edit-store-update" onClick={handleUpdate} disabled={busy}>Update Store</button>
                <button className="buttonEdit deleteEdit" data-testid="edit-store-delete" onClick={handleDelete} disabled={busy}>Delete Store</button>
            </div>
        </div>
    )
}

export const EditStorePage = () => {

    const params = useParams();
    const [store, setStore] = useState<Store>();
    const [error, setError] = useState("");

    useEffect(() => {
        const fetchStore = async () => {
            const res = await authClient.get(`/Store/GetStoreById/${params.id}`);
            return new Store(res.data.data);
        }

        fetchStore()
            .then(s => setStore(s))
            .catch(e => setError(e.response?.data ?? e.message ?? "Failed to load store"));

    }, [params.id])

    if (error)
        return <h2 className="editStoreMessage editStoreError">{error}</h2>

    if (!store)
        return <h2 className="editStoreMessage">Loading store...</h2>

    return <EditStoreForm store={store}/>
}
