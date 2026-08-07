import "../styles/AddStoreForm.css"
import {useState} from "react";
import authClient from "../api/AuthClient.ts";
import * as React from "react";

export const AddStoreForm = () => {

    const [storeName, setStoreName] = useState("");
    const [storeAddress, setStoreAddress] = useState("");
    const [storeCity, setStoreCity] = useState("");
    const [storeContact, setStoreContact] = useState("");
    const [storeOpening, setStoreOpening] = useState("");
    const [storeClosing, setStoreClosing] = useState("");
    const [error, setError] = useState("");
    const [message, setMessage] = useState("");
    
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
            })
        }
        catch(e: any) {
            setError(e.response?.data ?? e.message ?? "Undefined error");
            return;
        }
        setMessage("Added store \"" + result?.data.data.name + "\"");
        
        setTimeout(()=>{
            setMessage("");
        }, 2000);
        
        e.target.reset();
        
        console.log("Added store \"" + result?.data.data.name + "\"");
    }
    
    
    return (
        <form onSubmit={addStore} className="addStoreForm">
            <input type="text" 
                   placeholder="Store Name" 
                   onChange={(e) => {
                       setStoreName(e.target.value);
                   }}></input>
            <input type="text" 
                   placeholder="Address"
                   onChange={(e) => {
                       setStoreAddress(e.target.value);
                   }}></input>
            <input type="text"
                   placeholder="City"
                   onChange={(e) => {
                       setStoreCity(e.target.value);
                   }}></input>
            <input type="text" 
                   placeholder="Contact Number"
                   onChange={(e)=>{
                       setStoreContact(e.target.value);
                   }}></input>
            <input type="text" 
                   placeholder="Opening Hours HH:MM"
                   onBlur={(e) => {
                       e.target.value = validateTime(e.target.value);
                       setStoreOpening(e.target.value);
                   }}></input>
            <input type="text" 
                   placeholder="Closing Hours HH:MM"
                   onBlur={(e) => {
                       e.target.value = validateTime(e.target.value);
                       setStoreClosing(e.target.value);
                   }}></input>
            <button type="submit">Add Store</button>
            <h1 style={{color: "indianred"}}>{error}</h1>
            <h1 style={{color: "lightgreen"}}>{message}</h1>

        </form>
    )
}