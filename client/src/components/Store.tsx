import axios from "axios";
import {useEffect, useState} from "react";
import Product from "../models/Product.ts";
import {ProductCard} from "./ProductCard.tsx";
import "../styles/Store.css"
import {Filters} from "./Filters.tsx";

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
        
        const cached = checkCache();
        
        if (cached) {
            const result = JSON.parse(cached);
            finalizeData(result);
        }
        
        else{
            /* debouncing
             * sets a timer to execute the query, but resets it if its 
             * called again before the timer runs out, and discards the
             * call that was supposed to happen, and instead starts a new timer 
             */
            const timer = setTimeout(() =>
                    getProducts().then(res => {
                        finalizeData(res);
                        const cacheKey = '?'+ res.request.responseURL.split('?').at(-1)
                        localStorage.setItem(cacheKey, JSON.stringify({data: res.data, time: Date.now()}));
                    })
                        .catch(err => console.log(err))
                , 500)
            return () => clearTimeout(timer);
        }
        
    }, [change]);

    /* currently uses local storage, could change to redis as to not overload local storage */
    const checkCache = () => {
        const params = {
            page: filters.currentPage,
            items: filters.items,
            search: filters.search,
            type: filters.type,
            priceLow: filters.priceLow,
            priceHigh: filters.priceHigh
        }
        const url = axios.getUri({url:"", params});
        
        return localStorage.getItem(url);
        
    }
    
    const getProducts = async () => {
        return await axios.get("http://localhost:1738/api/Product/GetProductsFiltered", {
            params: {
                page: filters.currentPage,
                items: filters.items,
                search: filters.search,
                type: filters.type,
                priceLow: filters.priceLow,
                priceHigh: filters.priceHigh
            }
        });
    }
    
    const finalizeData = (result:any) =>{
        let temp = new Array<Product>();
        setFilters({...filters, pages: result.data.pages, currentPage:1});
        result.data.data.forEach((item: any) => {
            temp.push(new Product(item));
        })
        setProducts(temp);
    }
    
    return (
        <>
            <Filters params={{filters, setFilters, change, setChange}}/>
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