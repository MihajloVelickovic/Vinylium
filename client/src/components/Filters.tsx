import { useState } from "react";
import "../styles/Filters.css"
import Product from "../models/Product";
import Search from "../assets/search.svg"
import axios from "axios";

export const Filters = ({setProducts}) => {

    const [search, setSearch] = useState("");
    const [type, setType] = useState(0);
    const [priceLow, setPriceLow] = useState("");
    const [priceHigh, setPriceHigh] = useState("");
    
    const handleSubmit = async (e) => {
        e.preventDefault();
        try{
            const res = await axios.get("http://localhost:1738/api/Product/GetProductsFiltered", {
                params: {
                    title: search,
                    type: type != 0 ? type : null,
                    priceLow: priceLow ? priceLow : null,
                    priceHigh: priceHigh ? priceHigh : null,
                }
            })
            
            const temp = new Array<Product>();
            res.data.data.forEach((p: any) => temp.push(new Product(p)));
            setProducts(temp);
            
        }
        catch(err){
            console.log(err);
        }
        
    }
    
    return (
        <form className="filters" onSubmit={handleSubmit}>
            <label>Per Page</label>
            <select onChange={(t) => {
                setType(t.target.selectedIndex)
            }}>
                {
                    [20, 30, 40].map((item) => {
                        return <option>{item}</option>
                    })
                }
            </select>
            <input type="text" placeholder="Title" 
            onInput={(e) => setSearch(e.currentTarget.value)}></input>
            <select onChange={(t) => {
                setType(t.target.selectedIndex)
            }}>
                <option selected>Type</option>
                {
                    [0, 1, 2].map((item) => {
                        return <option>{Product.evaluateType(item)}</option>
                    })
                }
            </select>
            <input placeholder="Price from" type="number"
            onInput={(e) => {setPriceLow(e.currentTarget.value)}}></input>
            <input placeholder="Price to" type="number"
            onInput={(e) => setPriceHigh(e.currentTarget.value)}></input>
            
            <input type="image" alt="Search" src={Search}></input>
            
        </form>
    )
}