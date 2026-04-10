import axios from "axios";
import {useContext, useEffect, useState} from "react";
import Product from "../models/Product.ts";
import {ProductCard} from "./ProductCard.tsx";
import "../styles/Store.css"
import {Filters} from "./Filters.tsx";
import User from "../models/User.ts";

interface IFilter{
    pages:number;
    currentPage: number;
    items: number;
    search: string;
    type: null | number;
    priceLow:string
    priceHigh: string;
}

const Store = () => {
    
    const [products, setProducts] = useState<Product[]>([]);
    const [change, setChange] = useState(false);
    
    const [filters, setFilters] = useState<IFilter>({
        pages: 0, 
        currentPage: 1, 
        items: 20, 
        search: "", 
        type: null,
        priceLow: "",
        priceHigh: ""
    });

    useEffect(() => {
        // debouncing
        // sets a timer to execute the query, but resets it if its 
        // called again before the timer runs out, and discards the
        // call that was supposed to happen, and instead starts a new timer
        let timer = setTimeout(() => {
            handleSearch().then(res => setProducts(res))
                          .catch(err => console.log(err))
        }, 500)
        
        return () => {clearTimeout(timer);}        
    }, [change]);
    
    const handleSearch = async () =>{
        let result = await axios.get("http://localhost:1738/api/Product/GetProductsFiltered", {
            params:{
                page: filters.currentPage,
                items: filters.items,
                search: filters.search,
                type: filters.type,
                priceLow: filters.priceLow,
                priceHigh: filters.priceHigh
            }
        });
        let temp = new Array<Product>();
        setFilters({...filters, pages:result.data.pages});
        result.data.data.forEach((item: any) => {
            temp.push(new Product(item));
        })
        return temp;
    }
    
    return (
        <>
            <Filters params={{filters, setFilters, handleSearch, change, setChange}}/>
            <div className="products">
                {
                    products.map((product) => {
                        return <ProductCard product={product}/>
                    })
                }
            </div>
            
        </>
    )
}
export default Store