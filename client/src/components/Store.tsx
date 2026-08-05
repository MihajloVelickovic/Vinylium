import axios from "axios";
import {type InputEventHandler, useCallback, useEffect, useRef, useState} from "react";
import Product from "../models/Product.ts";
import {ProductCard} from "./ProductCard.tsx";
import "../styles/Store.css"
import {Filters} from "./Filters.tsx";
import client from "../api/Client.ts";

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
    const searchRef = useRef<HTMLInputElement>(null);
    
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
        /* debouncing
         * sets a timer to execute the query, but resets it if its 
         * called again before the timer runs out, and discards the
         * call that was supposed to happen, and instead starts a new timer 
         */
        const timer = setTimeout(() =>
                getProducts().then(res => {
                    finalizeData(res);
                })
                    .catch(err => console.log(err))
            , 100)
        return () => clearTimeout(timer);
    }, [change]);

    useEffect(() => {
        window.addEventListener("keydown", handleKeyPress);
        return() => {
            window.removeEventListener("keydown", handleKeyPress);
        }
    }, []);

    const handleKeyPress = useCallback((e) => {

        const currentElement = document.activeElement;
        /* forward slash and division */
        if ((e.which === 111 || e.which == 191) && document.activeElement !== searchRef.current 
                                                && !(currentElement instanceof HTMLInputElement)) {
            e.preventDefault();
            searchRef.current!.focus();
        }
        
        /* escape */
        if(e.which === 27 && currentElement instanceof HTMLInputElement){
            e.preventDefault();
            currentElement.blur();
        }
    },[])
    
    const getProducts = async () => {
        return await client.get("/Product/GetProductsFiltered", {
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
            <Filters searchRef={searchRef} params={{filters, setFilters, change, setChange}}/>
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