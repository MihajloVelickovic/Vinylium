import "../styles/AddStoreForm.css"
import {useEffect, useState} from "react";
import authClient from "../api/AuthClient.ts";
import * as React from "react";
import {useToast} from "./ToastContext.tsx";
import {apiError} from "../helpers/apiError.ts";

export const AddStoreForm = () => {

    const [storeName, setStoreName] = useState("");
    const [storeAddress, setStoreAddress] = useState("");
    const [storeCity, setStoreCity] = useState("");
    const [storeContact, setStoreContact] = useState("");
    const [storeOpening, setStoreOpening] = useState("");
    const [storeClosing, setStoreClosing] = useState("");
    const [warehouse, setWarehouse] = useState(false);
    const [warehouseVisible, setWarehouseVisible] = useState(true);
    const [error, setError] = useState("");
    const [message, setMessage] = useState("");
    const {notify} = useToast();
    
    useEffect(() => {
        const hasWarehouse = async () => {
            try {
                const res = await authClient.get("/Store/HasWarehouse");
                return res.data.data;
            }
            catch(e: any){
                setError(e.response?.data ?? e.message ?? "Undefined error");
                return;
            }
        }
        hasWarehouse().then(res => setWarehouseVisible(!res));
    }, [])
    
    const validateTime = (time) => {
        const timeRegex = /^[0-9]{2}:[0-9]{2}$/
        if(!timeRegex.test(time))
            return "00:00";
        const splitTime = time.split(':');
        if(parseInt(splitTime[0], 10) > 24)
            return "00:00";
        if(parseInt(splitTime[1], 10) > 60)
            return "00:00";
        return time;
    }

    const addStore = async (e) => {
        e.preventDefault();
        let result;
        try {
            result = await authClient.post("/Store/CreateStore", {
                name: storeName,
                address: storeAddress,
                city: storeCity,
                contactNumber: storeContact,
                openingHours: storeOpening,
                closingHours: storeClosing,
                isWarehouse: warehouse,
            })
        }
        catch(e: any) {
            const failure = apiError(e, "Undefined error");
            setError(failure);
            notify(failure, "error");
            return;
        }

        const added = result?.data.data.name;

        setError("");
        setMessage("Added store \"" + added + "\"");
        notify(`Added store "${added}"`);

        setTimeout(()=>{
            setMessage("");
        }, 2000);

        e.target.reset();
        setWarehouseVisible(warehouse ?  false : warehouseVisible);
    }
    
    return (
        <form onSubmit={addStore} className="addStoreForm">
            <input type="text" 
                   data-testid="store-name"
                   placeholder="Store Name" 
                   onChange={(e) => {
                       setStoreName(e.target.value);
                   }}></input>
            <input type="text" 
                   data-testid="store-address"
                   placeholder="Address"
                   onChange={(e) => {
                       setStoreAddress(e.target.value);
                   }}></input>
            <input type="text"
                   data-testid="store-city"
                   placeholder="City"
                   onChange={(e) => {
                       setStoreCity(e.target.value);
                   }}></input>
            <input type="text" 
                   data-testid="store-contact"
                   placeholder="Contact Number"
                   onChange={(e)=>{
                       setStoreContact(e.target.value);
                   }}></input>
            <input type="text" 
                   data-testid="store-opening"
                   placeholder="Opening Hours HH:MM"
                   onBlur={(e) => {
                       e.target.value = validateTime(e.target.value);
                       setStoreOpening(e.target.value);
                   }}></input>
            <input type="text" 
                   data-testid="store-closing"
                   placeholder="Closing Hours HH:MM"
                   onBlur={(e) => {
                       e.target.value = validateTime(e.target.value);
                       setStoreClosing(e.target.value);
                   }}></input>
            {warehouseVisible && 
                <div className="item">
                    <p>Warehouse</p>
                    <input type="checkbox"
                           data-testid="store-warehouse"
                           checked={warehouse}
                           onChange={(e) => {
                               setWarehouse(e.target.checked);
                           }}/>
                </div>
            }
            <button type="submit" data-testid="store-submit">Add Store</button>
            <h1 data-testid="store-error" style={{color: "indianred"}}>{error}</h1>
            <h1 data-testid="store-message" style={{color: "lightgreen"}}>{message}</h1>

        </form>
    )
}